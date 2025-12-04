namespace ResonanceSimulation.Core;

/// <summary>
/// Pääsimulaattori joka yhdistää SPH + DEM + Geometria + Mittaukset.
/// </summary>
public class Simulator
{
    private readonly SimulationConfig _config;
    private readonly TankGeometry _tank;
    private readonly SPHSolver _sphSolver;
    private readonly DEMSolver _demSolver;
    private readonly VerletIntegrator _integrator;
    private readonly Measurements _measurements;

    private List<SPHParticle> _fluidParticles = new();
    private List<DEMParticle> _damperParticles = new();

    private double _currentTime = 0.0;
    private int _step = 0;

    public Simulator(SimulationConfig config)
    {
        _config = config;
        _tank = new TankGeometry(config.TankWidth, config.TankHeight, config.Amplitude, config.Frequency);
        _sphSolver = new SPHSolver(config);
        _demSolver = new DEMSolver(config);
        _integrator = new VerletIntegrator(config.TimeStep);
        _measurements = new Measurements(_tank, config);
    }

    /// <summary>
    /// Alusta partikkelit.
    /// </summary>
    public void Initialize()
    {
        InitializeFluid();
        
        if (_config.EnableDamper)
        {
            InitializeDamper();
        }

        Console.WriteLine($"Initialized: {_fluidParticles.Count} SPH particles, {_damperParticles.Count} DEM particles");
    }

    /// <summary>
    /// Alusta nestepartikkelit (SPH) säännölliselle hilalle.
    /// </summary>
    private void InitializeFluid()
    {
        double spacing = _config.ParticleSpacing;
        double waterHeight = _tank.Height * _config.FillRatio;
        double particleMass = _config.RestDensity * spacing * spacing; // 2D: massa per yksikkösyvyys

        for (double x = _tank.MinX + spacing / 2; x < _tank.MaxX; x += spacing)
        {
            for (double y = _tank.MinY + spacing / 2; y < _tank.MinY + waterHeight; y += spacing)
            {
                var particle = new SPHParticle(
                    new Vector2D(x, y),
                    particleMass,
                    _config.SmoothingLength
                );

                _fluidParticles.Add(particle);
            }
        }
    }

    /// <summary>
    /// Alusta DEM-partikkelit (granulaari-damper) pohjakotelon.
    /// </summary>
    private void InitializeDamper()
    {
        double radius = _config.ParticleDiameter / 2.0;
        double spacing = _config.ParticleDiameter * 1.1; // Pieni väli vältetään päällekkäisyyttä

        // Laske kuinka monta palloa tarvitaan massasuhteen saavuttamiseksi
        double fluidMass = _fluidParticles.Sum(p => p.Mass);
        double targetDamperMass = fluidMass * _config.DamperMassRatio;

        double particleVolume = Math.PI * radius * radius;
        double particleMass = particleVolume * _config.ParticleDensity;
        int targetCount = (int)(targetDamperMass / particleMass);

        // Sijoita pallot pohjakoteon säännölliselle hilalle
        int count = 0;
        for (double x = _tank.MinX + radius; x < _tank.MaxX - radius && count < targetCount; x += spacing)
        {
            for (double y = _tank.MinY + radius; y < _tank.MinY + _config.DamperHeight && count < targetCount; y += spacing)
            {
                var particle = new DEMParticle(
                    new Vector2D(x, y),
                    radius,
                    _config.ParticleDensity,
                    _config.YoungsModulus,
                    _config.PoissonRatio,
                    _config.RestitutionCoeff,
                    _config.FrictionCoeff
                );

                _damperParticles.Add(particle);
                count++;
            }
        }

        Console.WriteLine($"Damper: {_damperParticles.Count} particles, total mass = {_damperParticles.Sum(p => p.Mass):F3} kg");
    }

    /// <summary>
    /// Suorita yksi aikaaskel.
    /// </summary>
    public void Step()
    {
        // 1. Päivitä tankin liike
        _tank.UpdateMotion(_currentTime);

        // 2. Nollaa voimat
        foreach (var p in _fluidParticles) p.ResetForces();
        foreach (var p in _damperParticles) p.ResetForces();

        // 3. SPH-laskenta (tiheys, paine, voimat)
        _sphSolver.Step(_fluidParticles);

        // 4. DEM-laskenta (kontaktivoimat)
        if (_damperParticles.Any())
        {
            _demSolver.ComputeContacts(_damperParticles);
            _demSolver.ComputeWallContacts(_damperParticles, _tank);
        }

        // 5. SPH–DEM kytkinä (neste vaikuttaa palloihin ja päinvastoin)
        ComputeSPHDEMCoupling();

        // 6. Lisää ulkoiset voimat (painovoima + pseudovoima tankki-liikkeestä)
        Vector2D inertialForce = new Vector2D(-_tank.CurrentAcceleration, 0); // Pseudo-voima
        Vector2D effectiveGravity = _config.Gravity + inertialForce;

        foreach (var p in _fluidParticles)
        {
            p.ExternalForce = p.Mass * inertialForce;
        }

        // 7. Aikaintegroi
        _integrator.Integrate(_fluidParticles, effectiveGravity);
        if (_damperParticles.Any())
        {
            _integrator.Integrate(_damperParticles, effectiveGravity);
        }

        // 8. Rajaehdot (estä partikkeleita menemästä ulos tankista)
        ApplyBoundaryConditions();

        // 9. Mittaukset
        if (_step % _config.OutputInterval == 0)
        {
            _measurements.MeasureWallPressure(_fluidParticles, _currentTime);
            _measurements.MeasureFreeSurface(_fluidParticles, _currentTime);
            _measurements.MeasureKineticEnergy(_fluidParticles, _damperParticles, _currentTime);
        }

        _currentTime += _config.TimeStep;
        _step++;
    }

    /// <summary>
    /// SPH–DEM kytkentä: neste vaikuttaa palloihin (buoyancy + drag).
    /// </summary>
    private void ComputeSPHDEMCoupling()
    {
        foreach (var dem in _damperParticles)
        {
            // Etsi lähimmät SPH-partikkelit
            var nearby = _fluidParticles.Where(sph =>
            {
                double dist = (dem.Position - sph.Position).Length();
                return dist < dem.Radius + 2.0 * _config.SmoothingLength;
            }).ToList();

            if (!nearby.Any()) continue;

            // Laske keskimääräinen tiheys ja paine
            double avgDensity = nearby.Average(p => p.Density);
            double avgPressure = nearby.Average(p => p.Pressure);

            // Buoyancy (Archimedes)
            double demVolume = Math.PI * dem.Radius * dem.Radius;
            Vector2D buoyancy = _config.Gravity * avgDensity * demVolume * (-1.0);

            // Drag (yksinkertaistettu)
            Vector2D avgVelocity = new Vector2D(
                nearby.Average(p => p.Velocity.X),
                nearby.Average(p => p.Velocity.Y)
            );
            Vector2D relativeVelocity = dem.Velocity - avgVelocity;
            double dragCoeff = 0.5;
            Vector2D drag = -dragCoeff * relativeVelocity * relativeVelocity.Length() * dem.Radius;

            dem.FluidForce = buoyancy + drag;
        }
    }

    /// <summary>
    /// Rajaehdot: estä partikkelit menemästä tankista ulos.
    /// </summary>
    private void ApplyBoundaryConditions()
    {
        // SPH: peilaa partikkelit jos ne menevät ulos
        foreach (var p in _fluidParticles)
        {
            var localPos = _tank.GlobalToLocal(p.Position);

            if (localPos.X < _tank.MinX)
            {
                localPos = new Vector2D(_tank.MinX, localPos.Y);
                p.Velocity = new Vector2D(-p.Velocity.X * 0.5, p.Velocity.Y);
            }
            if (localPos.X > _tank.MaxX)
            {
                localPos = new Vector2D(_tank.MaxX, localPos.Y);
                p.Velocity = new Vector2D(-p.Velocity.X * 0.5, p.Velocity.Y);
            }
            if (localPos.Y < _tank.MinY)
            {
                localPos = new Vector2D(localPos.X, _tank.MinY);
                p.Velocity = new Vector2D(p.Velocity.X, -p.Velocity.Y * 0.5);
            }

            p.Position = _tank.LocalToGlobal(localPos);
        }

        // DEM: jo hoidettu seinäkontakteissa
    }

    /// <summary>
    /// Simuloi kokonaan.
    /// </summary>
    public void Run()
    {
        int totalSteps = (int)(_config.TotalTime / _config.TimeStep);
        int reportInterval = totalSteps / 20; // Raportti 20 kertaa

        Console.WriteLine($"Running simulation: {totalSteps} steps, dt={_config.TimeStep} s");

        for (int i = 0; i < totalSteps; i++)
        {
            Step();

            if (i % reportInterval == 0)
            {
                double progress = 100.0 * i / totalSteps;
                Console.WriteLine($"Progress: {progress:F1}% (t={_currentTime:F2} s)");
            }
        }

        Console.WriteLine("Simulation complete!");
    }

    /// <summary>
    /// Palauta mittaukset.
    /// </summary>
    public Measurements GetMeasurements() => _measurements;
}
