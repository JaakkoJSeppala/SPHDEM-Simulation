using System;
using System.Collections.Generic;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.SPH;
using ShipHydroSim.Core.DEM;
using ShipHydroSim.Core.Ships;

namespace ShipHydroSim.Core.Coupling;

/// <summary>
/// SPH-DEM coupling: calculates fluid forces on rigid bodies
/// </summary>
public class BoundaryForceCalculator
{
    private readonly double _searchRadius;
    
    public BoundaryForceCalculator(double searchRadius = 0.5)
    {
        _searchRadius = searchRadius;
    }
    
    /// <summary>
    /// Calculate pressure and viscous forces from SPH particles on rigid body
    /// </summary>
    public (Vector3 force, Vector3 torque) CalculateFluidForces(
        RigidBody body, 
        IReadOnlyList<Particle> particles)
    {
        Vector3 totalForce = Vector3.Zero;
        Vector3 totalTorque = Vector3.Zero;
        
        // Simplified boundary handling: particles near ship surface
        foreach (var p in particles)
        {
            if (p.IsBoundary) continue;
            
            // Check if particle is near ship
            Vector3 toParticle = p.Position - body.Position;
            double dist = toParticle.Length;
            
            if (dist > _searchRadius) continue;
            
            // Pressure force (acts normal to surface) - increased coupling strength
            Vector3 normal = toParticle.Normalized();
            double pressureForce = p.Pressure * p.Mass / (p.Density * p.Density) * 2.5; // Stronger coupling
            
            Vector3 force = -normal * pressureForce;
            totalForce += force;
            
            // Torque = r × F
            Vector3 r = p.Position - body.Position;
            totalTorque += Vector3.Cross(r, force);
        }
        
        return (totalForce, totalTorque);
    }
    
    /// <summary>
    /// Apply rigid body motion to SPH particles (two-way coupling)
    /// Particles near ship get velocity from ship motion
    /// </summary>
    public void ApplyRigidBodyMotionToFluid(
        RigidBody body,
        IReadOnlyList<Particle> particles)
    {
        foreach (var p in particles)
        {
            if (p.IsBoundary) continue;
            
            Vector3 toParticle = p.Position - body.Position;
            double dist = toParticle.Length;
            
            if (dist > _searchRadius) continue;
            
            // Velocity of point on rigid body: v_point = v_cm + ω × r
            Vector3 velocityAtPoint = body.Velocity + Vector3.Cross(body.AngularVelocity, toParticle);
            
            // Blend particle velocity with rigid body velocity (damping)
            double blendFactor = Math.Max(0, 1.0 - dist / _searchRadius);
            p.Velocity = p.Velocity * (1 - blendFactor) + velocityAtPoint * blendFactor;
        }
    }
}
