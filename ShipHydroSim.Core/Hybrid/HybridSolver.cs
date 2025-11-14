using System;
using System.Collections.Generic;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.SPH;
using ShipHydroSim.Core.DEM;
using ShipHydroSim.Core.Ships;
using ShipHydroSim.Core.Waves;
using ShipHydroSim.Core.Coupling;
using ShipHydroSim.Core.Spatial;

namespace ShipHydroSim.Core.Hybrid;

/// <summary>
/// Hybrid solver: SPH (fluid) + DEM (rigid bodies) + Waves
/// 
/// VALIDATION TESTS (Planned):
/// 1. Single particle sedimentation: v_terminal \u2248 \u221a(2mg/(\u03c1AC_d))
/// 2. Hydrostatic pressure: p(y) = \u03c1g(h - y)  (tolerance ~1%)
/// 3. Dam break: Compare to Martin & Moyce (1952) surge front position
/// 4. Wave generation: Verify amplitude \u00b10.5%, frequency \u00b11%
/// 5. Energy conservation: Monitor E_kin + E_pot drift over time
/// 
/// References: Monaghan (1992), Robinson (2014), Canelas et al. (2016)
/// </summary>
public class HybridSolver : ISimulationSolver
{
    private readonly List<Particle> _particles;
    private readonly List<RigidBody> _rigidBodies;
    private readonly SpatialHash _spatialHash;
    private readonly BoundaryForceCalculator _boundaryForces;
    
    // Performance settings
    public int MaxParticles { get; set; } = 10000;
    public double TargetFPS { get; set; } = 30.0;
    public bool AdaptiveTimeStep { get; set; } = true;
    private double _targetStepTime = 33.3; // ms (30 FPS)
    private System.Diagnostics.Stopwatch _stepTimer = new();
    
    // SPH parameters
    public double SmoothingLength { get; set; } = 0.2;
    public double RestDensity { get; set; } = 1000.0;
    public double Stiffness { get; set; } = 20000.0;
    public double Viscosity { get; set; } = 0.05;
    public double SurfaceTension { get; set; } = 0.01; // Reduced for performance (was 0.0728)
    public bool EnableSurfaceTension { get; set; } = false; // can disable entirely for speed
    public Vector3 Gravity { get; set; } = new(0, -9.81, 0);
    public double TimeStep { get; set; } = 0.001;
    public double MaxTimeStep { get; set; } = 0.0025; // upper clamp for adaptive dt
    public double NeighborRadiusFactor { get; set; } = 2.0; // kernel support radius multiplier (2.0 for cubic spline)
    public bool UseLinearEOS { get; set; } = true; // faster than Tait gamma
    public double SoundSpeed { get; set; } = 25.0; // c for linear EOS, choose ~10x expected max fluid speed
    public double BaseWaterLevel { get; set; } = 2.0; // Mean still-water surface Y
    // Domain (tank) boundaries for fluid containment
    public Vector3 DomainMin { get; set; } = new Vector3(-10.0, 0.0, -10.0);
    public Vector3 DomainMax { get; set; } = new Vector3(10.0, 4.0, 10.0);
    
    // Porosity field (future improvement for solid volume fraction)
    // φ(x): fraction of fluid at position x, φ=1 (pure fluid), φ=0 (solid)
    // Modified density: ρ_eff = φ * ρ_fluid
    // Currently: Not implemented (φ=1 everywhere), identified for future work
    public bool EnablePorosityField { get; set; } = false; // Future: φ(x) near boundaries
    
    // Stability parameters
    public double MaxWaterHeight { get; set; } = 4.0; // Hard cap for splashes
    public double SplashDampingFactor { get; set; } = 0.6; // Velocity retained on hitting cap
    public double CFLFactor { get; set; } = 0.4; // CFL-based adaptive timestep factor
    public double MaxVelocityObserved { get; private set; } = 0.0; // For diagnostics
    
    // Wave generator
    public IWaveGenerator? WaveGenerator { get; set; }
    public bool EnableWaves { get; set; } = false;
    
    // Simulation state
    public double CurrentTime { get; private set; }
    public int StepCount { get; private set; }
    
    public int ParticleCount => _particles.Count;
    public IReadOnlyList<Particle> Particles => _particles;
    public IReadOnlyList<RigidBody> RigidBodies => _rigidBodies;
    
    public HybridSolver()
    {
        _particles = new List<Particle>();
        _rigidBodies = new List<RigidBody>();
        _spatialHash = new SpatialHash(SmoothingLength * 2.0);
        _boundaryForces = new BoundaryForceCalculator(SmoothingLength * 2.0);
        _targetStepTime = 1000.0 / TargetFPS;
    }
    
    public void AddParticle(Particle particle)
    {
        if (_particles.Count < MaxParticles)
        {
            _particles.Add(particle);
        }
    }
    public void AddRigidBody(RigidBody body) => _rigidBodies.Add(body);
    
    public void Step()
    {
        _stepTimer.Restart();
        
        // Adaptive time step: min(dt_performance, dt_CFL, dt_DEM, MaxTimeStep)
        // dt_CFL = CFL * h / max|v|  (stability, handled in ApplyStabilityConstraints)
        // dt_DEM = sqrt(m/k)  (for contact mechanics, when implemented)
        // Synchronization: Both SPH and DEM use same Δt for momentum conservation
        if (AdaptiveTimeStep && StepCount > 10)
        {
            double avgStepTime = _stepTimer.Elapsed.TotalMilliseconds;
            if (avgStepTime > _targetStepTime * 1.5)
            {
                TimeStep = Math.Max(0.0005, TimeStep * 0.95);
            }
            else if (avgStepTime < _targetStepTime * 0.5)
            {
                TimeStep = Math.Min(MaxTimeStep, TimeStep * 1.05);
            }
        }
        
        // 1. Apply wave forcing to fluid
        if (EnableWaves && WaveGenerator != null)
        {
            ApplyWaveForcing();
        }
        
        // 2. SPH: Update spatial hash
        _spatialHash.Clear();
        foreach (var p in _particles)
            _spatialHash.Insert(p);
        
        // 3. SPH: Compute density and pressure
        ComputeDensityAndPressure();
        
        // 4. SPH: Compute forces
        ComputeSPHForces();
        
        // 5. DEM: Compute forces on rigid bodies (gravity + buoyancy)
        ComputeRigidBodyForces();
        
        // 6. Coupling: Fluid → Rigid body (pressure forces)
        ApplyCouplingForces();
        
        // 7. Integrate SPH particles
        IntegrateParticles();
        ApplyStabilityConstraints();
        
        // 8. Integrate rigid bodies (6DOF)
        IntegrateRigidBodies();
        
        // 9. Coupling: Rigid body → Fluid (drag particles with ship)
        if (_rigidBodies.Count > 0)
        {
            _boundaryForces.ApplyRigidBodyMotionToFluid(_rigidBodies[0], _particles);
        }
        
            CurrentTime += TimeStep;
            StepCount++;
            
            // CPU-rajoitin: odota jos step oli liian nopea
            _stepTimer.Stop();
            double elapsedMs = _stepTimer.Elapsed.TotalMilliseconds;
            if (elapsedMs < _targetStepTime)
            {
                int sleepMs = (int)(_targetStepTime - elapsedMs);
                if (sleepMs > 0)
                {
                    System.Threading.Thread.Sleep(sleepMs);
                }
            }
        }    private void ApplyWaveForcing()
    {
        if (WaveGenerator == null) return;
        
        foreach (var p in _particles)
        {
            if (p.IsBoundary) continue;
            
            // Wave properties at particle horizontal location
            double waveElev = WaveGenerator.GetElevation(p.Position.X, p.Position.Z, CurrentTime);
            double targetY = BaseWaterLevel + waveElev;
            Vector3 waveVel = WaveGenerator.GetVelocity(p.Position.X, p.Position.Y, p.Position.Z, CurrentTime);

            // Surface band around mean level
            bool nearSurface = Math.Abs(p.Position.Y - BaseWaterLevel) < 0.7;

            // Gentle vertical restoring toward target surface height
            double heightDiff = targetY - p.Position.Y;
            double verticalGain = nearSurface ? 6.0 : 2.0;
            p.Acceleration += new Vector3(0, heightDiff * verticalGain, 0);

            // Horizontal velocity coupling with decay by depth
            double depth = Math.Max(0.0, BaseWaterLevel - p.Position.Y);
            double decay = Math.Exp(-depth / Math.Max(0.5, SmoothingLength));
            double blend = (nearSurface ? 0.5 : 0.2) * decay;
            p.Acceleration += (waveVel - p.Velocity) * blend / Math.Max(1e-4, TimeStep);
        }
    }
    
    private void ComputeDensityAndPressure()
    {
        // SPH density summation: ρᵢ = Σⱼ mⱼ W(rᵢⱼ, h)  (Monaghan 1992, Eq. 2)
        // Note: Future improvement - porosity field φ(x) for solid volume fraction
        //       Modified density: ρ_eff = φ * ρ_fluid (for porous media)
        double kernelRadius = SmoothingLength * NeighborRadiusFactor;
        foreach (var pi in _particles)
        {
            double density = 0.0;
            var neighbors = _spatialHash.FindNeighbors(pi.Position, kernelRadius);
            
            foreach (var pj in neighbors)
            {
                double r = (pi.Position - pj.Position).Length;
                density += pj.Mass * KernelFunctions.CubicSpline(r, SmoothingLength);
            }
            
            pi.Density = Math.Max(density, RestDensity * 0.01);
            
            if (UseLinearEOS)
            {
                // Linearized equation of state: p = c^2 (rho - rho0)
                pi.Pressure = SoundSpeed * SoundSpeed * (pi.Density - RestDensity);
            }
            else
            {
                // Tait equation
                double gamma = 7.0;
                pi.Pressure = Stiffness * (Math.Pow(pi.Density / RestDensity, gamma) - 1.0);
            }
        }
    }
    
    private void ComputeSPHForces()
    {
        // SPH force computation (Monaghan 1992)
        // Pressure: -m_j (p_i/ρ_i² + p_j/ρ_j²) ∇W (symmetric formulation, Eq. 10)
        // Viscosity: XSPH variant (Eq. 14)
        // Surface tension: Color field method (optional, Morris 2000)
        double kernelRadius = SmoothingLength * NeighborRadiusFactor;
        foreach (var pi in _particles)
        {
            if (pi.IsBoundary) continue;
            
            Vector3 pressureForce = Vector3.Zero;
            Vector3 viscosityForce = Vector3.Zero;
            
            // Surface detection (quick check)
            bool isSurface = EnableSurfaceTension && pi.Position.Y > 1.0 && pi.Density < RestDensity * 0.9;
            
            Vector3 colorGradient = Vector3.Zero;
            double colorLaplacian = 0.0;
            
            var neighbors = _spatialHash.FindNeighbors(pi.Position, kernelRadius);
            
            foreach (var pj in neighbors)
            {
                if (pi.Id == pj.Id) continue;
                
                Vector3 rij = pi.Position - pj.Position;
                double r = rij.Length;
                if (r < 1e-6) continue;
                
                Vector3 rijNorm = rij / r;
                double gradW = KernelFunctions.CubicSplineGradient(r, SmoothingLength);
                
                // Pressure (symmetric SPH)
                double pressureTerm = (pi.Pressure / (pi.Density * pi.Density) + 
                                      pj.Pressure / (pj.Density * pj.Density));
                pressureForce -= pj.Mass * pressureTerm * gradW * rijNorm;
                
                // Viscosity (XSPH variant)
                Vector3 vij = pi.Velocity - pj.Velocity;
                viscosityForce += Viscosity * pj.Mass * vij / pj.Density * gradW;
                
                // Surface tension only for surface particles (performance)
                if (EnableSurfaceTension && isSurface)
                {
                    double W = KernelFunctions.CubicSpline(r, SmoothingLength);
                    colorGradient += pj.Mass / pj.Density * gradW * rijNorm;
                    colorLaplacian += pj.Mass / pj.Density * (gradW / r);
                }
            }
            
            Vector3 surfaceTensionForce = Vector3.Zero;
            if (EnableSurfaceTension && isSurface)
            {
                double colorGradMag = colorGradient.Length;
                if (colorGradMag > 0.01 / SmoothingLength)
                {
                    Vector3 normal = colorGradient / colorGradMag;
                    surfaceTensionForce = -SurfaceTension * colorLaplacian * normal;
                }
            }
            
            pi.Acceleration = (pressureForce + viscosityForce + surfaceTensionForce) / pi.Density + Gravity;
        }
    }
    
    private void ComputeRigidBodyForces()
    {
        foreach (var body in _rigidBodies)
        {
            if (body.IsStatic) continue;
            
            // Reset forces
            body.Force = Vector3.Zero;
            body.Torque = Vector3.Zero;
            
            // Gravity
            body.Force += Gravity * body.Mass;
            
            // Buoyancy (Archimedes)
            if (body is ShipRigidBody ship)
            {
                // Sample distributed water level under hull for excitation
                double waveElevCenter = EnableWaves && WaveGenerator != null
                    ? WaveGenerator.GetElevation(ship.Position.X, ship.Position.Z, CurrentTime)
                    : 0.0;
                double waterLevelCenter = BaseWaterLevel + waveElevCenter;
                body.Force += ship.CalculateBuoyancy(waterLevelCenter);
                body.Torque += ship.CalculateHydrostaticRestoringTorque(waterLevelCenter);

                // Differential buoyancy for roll & pitch (four corner sampling)
                // Theory: Wave elevation varies across hull → pressure differential → torque
                // Τ = Σ_i r_i × F_i, where F_i = ρ g A_i Δh_i (local submergence)
                // Net force removed (avg subtraction) to isolate pure torque excitation
                if (EnableWaves && WaveGenerator != null)
                {
                    // Local hull corners (assume small angles → ignore rotation for sampling)
                    double halfL = ship.Length / 2.0;
                    double halfB = ship.Beam / 2.0;
                    var corners = new (double dx, double dz)[] {
                        (-halfL, -halfB), // aft-port
                        (-halfL,  halfB), // aft-starboard
                        ( halfL, -halfB), // fwd-port
                        ( halfL,  halfB)  // fwd-starboard
                    };
                    double rhoWater = 1000.0;
                    double g = 9.81;
                    double panelArea = (ship.Length/2.0) * (ship.Beam/2.0); // coarse panel area
                    // First compute local vertical forces at corners
                    double[] F = new double[4];
                    int idx = 0;
                    foreach (var c in corners)
                    {
                        double wx = ship.Position.X + c.dx;
                        double wz = ship.Position.Z + c.dz;
                        double waveElev = WaveGenerator.GetElevation(wx, wz, CurrentTime);
                        double localWaterLevel = BaseWaterLevel + waveElev;
                        double hullBottom = ship.Position.Y - ship.Draft / 2.0;
                        double subDepth = Math.Max(0.0, localWaterLevel - hullBottom);
                        double localBuoyForceMag = subDepth > 0 ? rhoWater * g * subDepth * panelArea * 0.2 : 0.0;
                        F[idx++] = localBuoyForceMag;
                    }
                    // Remove net force, keep only differential to produce torque without extra lift
                    double avg = (F[0] + F[1] + F[2] + F[3]) / 4.0;
                    idx = 0;
                    foreach (var c in corners)
                    {
                        double dF = F[idx++] - avg;
                        if (Math.Abs(dF) < 1e-9) continue;
                        var rLocal = new Vector3(c.dx, 0, c.dz);
                        var rWorld = ship.Orientation.Rotate(rLocal);
                        var force = new Vector3(0, dF, 0);
                        body.Torque += Vector3.Cross(rWorld, force);
                    }
                }
            }
        }
    }
    
    private void ApplyCouplingForces()
    {
        foreach (var body in _rigidBodies)
        {
            if (body.IsStatic) continue;
            
            // Fluid → Rigid body: pressure and drag forces
            var (fluidForce, fluidTorque) = _boundaryForces.CalculateFluidForces(body, _particles);
            
            body.Force += fluidForce;
            body.Torque += fluidTorque;
        }
    }
    
    private void IntegrateParticles()
    {
        foreach (var p in _particles)
        {
            if (p.IsBoundary) continue;
            
            p.Velocity += p.Acceleration * TimeStep;
            p.Position += p.Velocity * TimeStep;
            
            // Boundary conditions
            ApplyBoundaryConditions(p);
        }
    }

    // Constrain excessive splashes & apply CFL adaptive timestep
    private void ApplyStabilityConstraints()
    {
        double maxSpeed = 0.0;
        foreach (var p in _particles)
        {
            if (p.IsBoundary) continue;
            double speed = p.Velocity.Length;
            if (speed > maxSpeed) maxSpeed = speed;
            
            // Hard height cap for water domain
            if (p.Position.Y > MaxWaterHeight)
            {
                var pos = p.Position;
                var vel = p.Velocity;
                pos.Y = MaxWaterHeight;
                vel.Y = -vel.Y * SplashDampingFactor;
                vel.X *= 0.9;
                vel.Z *= 0.9;
                p.Position = pos;
                p.Velocity = vel;
            }
        }
        MaxVelocityObserved = maxSpeed;
        // CFL-based timestep reduction (optional – only shrink)
        double cflDt = CFLFactor * SmoothingLength / (maxSpeed + 1e-6);
        if (AdaptiveTimeStep && cflDt < TimeStep)
        {
            TimeStep = Math.Max(0.0004, cflDt); // Do not go below floor
        }
    }
    
    private void IntegrateRigidBodies()
    {
        foreach (var body in _rigidBodies)
        {
            if (body.IsStatic) continue;
            
            // Linear motion
            body.Velocity += body.Force * body.InverseMass * TimeStep;
            body.Position += body.Velocity * TimeStep;
            
            // Angular motion with world-space inertia handling
            var q = body.Orientation.Normalized();
            // Transform torque to body space, apply inverse inertia, then back to world
            Vector3 torqueBody = q.Conjugate().Rotate(body.Torque);
            Vector3 angularAccelBody = body.InverseInertiaTensor * torqueBody;
            Vector3 angularAccel = q.Rotate(angularAccelBody);
            body.AngularVelocity += angularAccel * TimeStep;
            
            // Update orientation (quaternion integration)
            double angle = body.AngularVelocity.Length * TimeStep;
            if (angle > 1e-6)
            {
                Vector3 axis = body.AngularVelocity.Normalized();
                Quaternion deltaQ = Quaternion.FromAxisAngle(axis, angle);
                body.Orientation = (deltaQ * body.Orientation).Normalized();
            }
            
            // Damping (hydrodynamic damping)
            if (body is ShipRigidBody ship)
            {
                body.Velocity *= (1.0 - ship.DampingCoeff * TimeStep);
                body.AngularVelocity *= (1.0 - ship.DampingCoeff * TimeStep);
                // Extract roll/pitch from orientation quaternion for UI
                q = body.Orientation;
                double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
                double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
                ship.Roll = Math.Atan2(sinr_cosp, cosr_cosp);
                double sinp = 2 * (q.W * q.Y - q.Z * q.X);
                if (Math.Abs(sinp) >= 1)
                    ship.Pitch = Math.CopySign(Math.PI / 2, sinp);
                else
                    ship.Pitch = Math.Asin(sinp);
                // Heave relative to base water level
                ship.Heave = ship.Position.Y - BaseWaterLevel;
            }
        }
    }
    
    private void ApplyBoundaryConditions(Particle p)
    {
        double damping = 0.5;
        Vector3 pos = p.Position;
        Vector3 vel = p.Velocity;
        // Reflective walls using configurable domain
        if (pos.X < DomainMin.X) { pos.X = DomainMin.X; vel.X *= -damping; }
        if (pos.X > DomainMax.X) { pos.X = DomainMax.X; vel.X *= -damping; }
        if (pos.Z < DomainMin.Z) { pos.Z = DomainMin.Z; vel.Z *= -damping; }
        if (pos.Z > DomainMax.Z) { pos.Z = DomainMax.Z; vel.Z *= -damping; }
        if (pos.Y < DomainMin.Y) { pos.Y = DomainMin.Y; vel.Y *= -damping; }
        // Cap vertical extent both by tank ceiling and splash height
        double topY = Math.Min(DomainMax.Y, MaxWaterHeight);
        if (pos.Y > topY) { pos.Y = topY; vel.Y *= -damping; }
        
        p.Position = pos;
        p.Velocity = vel;
    }
}
