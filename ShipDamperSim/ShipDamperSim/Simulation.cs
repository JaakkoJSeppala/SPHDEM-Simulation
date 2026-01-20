using ShipDamperSim.Core;
using System.Globalization;
using System.Text;

namespace ShipDamperSim;

public sealed class Simulation
{
    private readonly SimConfig _cfg;
    private readonly ShipRoll _ship;
    private DemEngine? _damper = null;
    private readonly SphFluid _fluid;

    public Simulation(SimConfig cfg)
    {
        _cfg = cfg;
        _ship = new ShipRoll(cfg.Ship);

        if (cfg.Damper.Enabled)
            _damper = new DemEngine();


        _fluid = new SphFluid(100);
        _fluid.SetWaveParams(cfg.WaveAmplitude, cfg.WaveFrequency);
    }

    // Expose SPH fluid particles for visualization
    public IReadOnlyList<SphFluid.Particle> FluidParticles => _fluid.Particles;

    // Expose damper particles for visualization (if damper exists)
    // (alustus siirretty propertyyn, ei kenttää)

    public IEnumerable<(float X, float Y, float Z)> DamperParticles =>
        _damper != null ? _damper.ParticlesForVisualization() : Array.Empty<(float, float, float)>();
    // Step the simulation by one step (for visualization)
    // Simple box hull: x=0, y in [0,1], z in [-0.2,0.2]
    // Laske painevoimat rungolle nykyisen asennon mukaan (heave, roll)
    private (double forceY, double moment) ComputeSphHullForces()
    {
        double forceY = 0.0;
        double moment = 0.0;
        // Laivan asento
        double phi = _ship.Phi;
        double y0 = _ship.Y;
        // Laatikko: x=0, y in [y0-0.5, y0+0.5], z in [-0.2,0.2]
        double hullX = 0.0, hullYmin = y0 - 0.5, hullYmax = y0 + 0.5, hullZmin = -0.2, hullZmax = 0.2;
        double areaPerParticle = 0.01;
        // Pyöritä hiukkaset rungon koordinaatistoon (vain roll)
        foreach (var p in _fluid.Particles)
        {
            // Käännä hiukkasen paikka laivan koordinaatistoon (roll)
            double xh = Math.Cos(-phi) * (p.Position.X - hullX) - Math.Sin(-phi) * (p.Position.Y - y0);
            double yh = Math.Sin(-phi) * (p.Position.X - hullX) + Math.Cos(-phi) * (p.Position.Y - y0);
            double zh = p.Position.Z;
            if (Math.Abs(xh) < 0.03 &&
                yh >= -0.5 && yh <= 0.5 &&
                zh >= hullZmin && zh <= hullZmax)
            {
                // Outward normal rungon sivulla: (-1,0,0) rungon koordinaatistossa
                // Palauta maailman koordinaatteihin
                var n = new Vec3(-Math.Cos(phi), -Math.Sin(phi), 0);
                var F = -p.Pressure * areaPerParticle * n;
                forceY += F.Y;
                // Moment originin ympäri (roll): r x F
                var r = new Vec3(p.Position.X, p.Position.Y, p.Position.Z);
                var M = new Vec3(
                    r.Y * F.Z - r.Z * F.Y,
                    r.Z * F.X - r.X * F.Z,
                    r.X * F.Y - r.Y * F.X
                );
                moment += M.X;
            }
        }
        return (forceY, moment);
    }

    public void StepOnce()
    {
        double dt = _cfg.Time.Dt;
        double t = 0; // Optionally track time if needed

        double mWave = WaveMoment(t);
        double mDamper = 0.0;
        if (_damper is not null)
        {
            _damper.Step((float)dt);
            mDamper = 0.0; // momentti voidaan laskea myöhemmin
        }
        _fluid.Step(dt);
        // SPH pressure forces on hull
        var (fluidForceY, mFluid) = ComputeSphHullForces();
        _ship.Step(dt, mWave + mFluid, mDamper, fluidForceY);
    }

    public void Run()
    {
        double dt = _cfg.Time.Dt;
        int nSteps = (int)Math.Ceiling(_cfg.Time.TEnd / dt);
        string csvPath = Path.Combine(_cfg.Output.OutputDir, _cfg.Output.CsvFile);
        using var sw = new StreamWriter(csvPath, false, Encoding.UTF8);
        sw.WriteLine("t,phi_rad,phi_deg,phiDot_rad_s,M_wave,M_damper,fluid_com_y,fluid_avg_density,damper_com_y,damper_com_z,damper_dissip,damper_contactwork,damper_dissip_total,damper_contactwork_total");
        var inv = CultureInfo.InvariantCulture;
        for (int step = 0; step <= nSteps; step++)
        {
            double t = step * dt;
            double mWave = WaveMoment(t);
            double mDamper = 0.0;
            double damperDissip = double.NaN, damperContact = double.NaN, damperDissipTot = double.NaN, damperContactTot = double.NaN;
            if (_damper is not null)
            {
                _damper.Step((float)dt);
                mDamper = 0.0; // momentti voidaan laskea myöhemmin
            }
            // Step SPH fluid
            _fluid.Step(dt);
            // Compute net Y-force from SPH fluid and convert to roll moment (simple demo: moment arm = 1.0)
            // SPH pressure forces on hull
            var (fluidForceY, mFluid) = ComputeSphHullForces();
            _ship.Step(dt, mWave + mFluid, mDamper, fluidForceY);
            if (step % _cfg.Time.LogEvery == 0)
            {
                double phiDeg = _ship.Phi * 180.0 / Math.PI;
                // Calculate fluid center of mass (Y) and average density for logging
                double comY = 0, avgRho = 0;
                int count = _fluid.Particles.Count;
                if (count > 0)
                {
                    foreach (var p in _fluid.Particles)
                    {
                        comY += p.Position.Y;
                        avgRho += p.Density;
                    }
                    comY /= count;
                    avgRho /= count;
                }
                // Damper center of mass (Y, Z)
                double damperComY = double.NaN, damperComZ = double.NaN;
                if (_damper != null)
                {
                    double sumY = 0, sumZ = 0; int n = 0;
                    foreach (var (x, y, z) in _damper.ParticlesForVisualization())
                    {
                        sumY += y;
                        sumZ += z;
                        n++;
                    }
                    if (n > 0)
                    {
                        damperComY = sumY / n;
                        damperComZ = sumZ / n;
                    }
                }
                sw.WriteLine(string.Join(",",
                    t.ToString("G17", inv),
                    _ship.Phi.ToString("G17", inv),
                    phiDeg.ToString("G17", inv),
                    _ship.PhiDot.ToString("G17", inv),
                    mWave.ToString("G17", inv),
                    mDamper.ToString("G17", inv),
                    comY.ToString("G17", inv),
                    avgRho.ToString("G17", inv),
                    damperComY.ToString("G17", inv),
                    damperComZ.ToString("G17", inv),
                    damperDissip.ToString("G17", inv),
                    damperContact.ToString("G17", inv),
                    damperDissipTot.ToString("G17", inv),
                    damperContactTot.ToString("G17", inv)
                ));
            }
        }
    }

    private double WaveMoment(double t)
    {
        if (!string.Equals(_cfg.Excitation.Type, "sine", StringComparison.OrdinalIgnoreCase))
            return 0.0;
        double w = 2.0 * Math.PI * _cfg.Excitation.FreqHz;
        return _cfg.Excitation.M0 * Math.Sin(w * t);
    }
}
