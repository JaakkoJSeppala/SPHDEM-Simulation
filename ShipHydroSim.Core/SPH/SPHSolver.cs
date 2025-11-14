using System;
using System.Collections.Generic;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.Spatial;

namespace ShipHydroSim.Core.SPH;

/// <summary>
/// SPH solver implementing WCSPH (Weakly Compressible SPH)
/// </summary>
public class SPHSolver
{
    private readonly List<Particle> _particles;
    private readonly SpatialHash _spatialHash;
    
    // SPH parameters
    public double SmoothingLength { get; set; } = 0.1;    // h
    public double RestDensity { get; set; } = 1000.0;     // ρ₀ (kg/m³)
    public double Stiffness { get; set; } = 10000.0;      // k (Pa)
    public double Viscosity { get; set; } = 0.01;         // μ (Pa·s)
    public Vector3 Gravity { get; set; } = new(0, -9.81, 0);
    public double TimeStep { get; set; } = 0.001;         // dt (s)
    
    public SPHSolver()
    {
        _particles = new List<Particle>();
        _spatialHash = new SpatialHash(SmoothingLength * 2.0);
    }

    public void AddParticle(Particle particle)
    {
        _particles.Add(particle);
    }

    public IReadOnlyList<Particle> Particles => _particles;

    /// <summary>
    /// Single simulation step
    /// </summary>
    public void Step()
    {
        // 1. Update spatial hash
        _spatialHash.Clear();
        foreach (var p in _particles)
            _spatialHash.Insert(p);

        // 2. Compute density and pressure
        ComputeDensityAndPressure();

        // 3. Compute forces
        ComputeForces();

        // 4. Integrate (Explicit Euler for simplicity, can use Verlet later)
        Integrate();
    }

    private void ComputeDensityAndPressure()
    {
        foreach (var pi in _particles)
        {
            double density = 0.0;
            var neighbors = _spatialHash.FindNeighbors(pi.Position, SmoothingLength * 2.0);

            foreach (var pj in neighbors)
            {
                double r = (pi.Position - pj.Position).Length;
                density += pj.Mass * KernelFunctions.CubicSpline(r, SmoothingLength);
            }

            pi.Density = Math.Max(density, RestDensity * 0.01); // Avoid division by zero
            
            // Tait equation: p = k * ((ρ/ρ₀)^γ - 1)
            double gamma = 7.0;
            pi.Pressure = Stiffness * (Math.Pow(pi.Density / RestDensity, gamma) - 1.0);
        }
    }

    private void ComputeForces()
    {
        foreach (var pi in _particles)
        {
            if (pi.IsBoundary) continue;

            Vector3 pressureForce = Vector3.Zero;
            Vector3 viscosityForce = Vector3.Zero;

            var neighbors = _spatialHash.FindNeighbors(pi.Position, SmoothingLength * 2.0);

            foreach (var pj in neighbors)
            {
                if (pi.Id == pj.Id) continue;

                Vector3 rij = pi.Position - pj.Position;
                double r = rij.Length;
                if (r < 1e-6) continue;

                Vector3 rijNorm = rij / r;
                double gradW = KernelFunctions.CubicSplineGradient(r, SmoothingLength);

                // Pressure force (symmetric)
                double pressureTerm = (pi.Pressure / (pi.Density * pi.Density) + 
                                      pj.Pressure / (pj.Density * pj.Density));
                pressureForce -= pj.Mass * pressureTerm * gradW * rijNorm;

                // Viscosity force (XSPH variant)
                Vector3 vij = pi.Velocity - pj.Velocity;
                viscosityForce -= Viscosity * pj.Mass * vij / pj.Density * gradW;
            }

            // Total acceleration
            pi.Acceleration = (pressureForce + viscosityForce) / pi.Density + Gravity;
        }
    }

    private void Integrate()
    {
        foreach (var p in _particles)
        {
            if (p.IsBoundary) continue;

            // Explicit Euler
            p.Velocity += p.Acceleration * TimeStep;
            p.Position += p.Velocity * TimeStep;

            // Simple boundary conditions (box: 0 to 10)
            ApplyBoundaryConditions(p);
        }
    }

    private void ApplyBoundaryConditions(Particle p)
    {
        double damping = 0.5; // Coefficient of restitution
        
        Vector3 pos = p.Position;
        Vector3 vel = p.Velocity;

        // X boundaries
        if (pos.X < 0) { pos.X = 0; vel.X *= -damping; }
        if (pos.X > 10) { pos.X = 10; vel.X *= -damping; }

        // Y boundaries
        if (pos.Y < 0) { pos.Y = 0; vel.Y *= -damping; }
        if (pos.Y > 10) { pos.Y = 10; vel.Y *= -damping; }

        // Z boundaries
        if (pos.Z < 0) { pos.Z = 0; vel.Z *= -damping; }
        if (pos.Z > 10) { pos.Z = 10; vel.Z *= -damping; }
        
        p.Position = pos;
        p.Velocity = vel;
    }
}
