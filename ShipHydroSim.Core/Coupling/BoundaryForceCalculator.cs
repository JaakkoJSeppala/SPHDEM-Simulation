using System;
using System.Collections.Generic;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.SPH;
using ShipHydroSim.Core.DEM;
using ShipHydroSim.Core.Spatial;

namespace ShipHydroSim.Core.Coupling;

/// <summary>
/// Theoretical SPH-DEM coupling implementation
/// 
/// Implements proper two-way momentum exchange:
/// 1. Fluid → Rigid body: Pressure + Drag forces via boundary particles
/// 2. Rigid body → Fluid: Reaction forces distributed via kernel weighting
/// 
/// References:
/// - Adami et al. (2012) "A generalized wall boundary condition for SPH"
/// - Canelas et al. (2016) "SPH–DCDEM model for arbitrary geometries in free surface solid–fluid flows"
/// - Robinson et al. (2014) "Fluid-particle flow simulations using two-way-coupled mesoscale SPH-DEM"
/// </summary>
public class BoundaryForceCalculator
{
    private readonly double _smoothingLength;
    private readonly double _kernelRadius; // Support radius = factor * h
    private readonly double _dragCoefficient;
    
    /// <summary>
    /// Boundary particles representing rigid body surface
    /// Updated each timestep with body transform
    /// </summary>
    public List<BoundaryParticle> BoundaryParticles { get; private set; }
    
    public BoundaryForceCalculator(double smoothingLength, double kernelRadiusFactor = 2.0, double dragCoeff = 0.5)
    {
        _smoothingLength = smoothingLength;
        _kernelRadius = smoothingLength * kernelRadiusFactor;
        _dragCoefficient = dragCoeff;
        BoundaryParticles = new List<BoundaryParticle>();
    }
    
    /// <summary>
    /// Initialize boundary shell for a rigid body
    /// </summary>
    public void InitializeBoundaryShell(RigidBody body, double length, double width, double height, double spacing)
    {
        BoundaryParticles = BoundaryShellGenerator.GenerateBoxHull(length, width, height, spacing);
        
        // Initial transform
        UpdateBoundaryTransforms(body);
    }
    
    /// <summary>
    /// Update boundary particle positions and normals from rigid body state
    /// Call every timestep before force calculation
    /// </summary>
    public void UpdateBoundaryTransforms(RigidBody body)
    {
        foreach (var bp in BoundaryParticles)
        {
            bp.UpdateTransform(body.Position, body.Orientation);
            bp.UpdateVelocity(body.Velocity, body.AngularVelocity, body.Position);
        }
    }
    
    /// <summary>
    /// Step 1: Extrapolate SPH field values to boundary particles
    /// Computes pressure and velocity at each boundary location using kernel interpolation
    /// 
    /// ρ_b = Σ_i (m_i * W(r_bi, h))
    /// p_b = Σ_i (p_i * W(r_bi, h)) / Σ_i W(r_bi, h)
    /// u_b = Σ_i (u_i * W(r_bi, h)) / Σ_i W(r_bi, h)
    /// </summary>
    public void ExtrapolateSPHFieldToBoundary(IReadOnlyList<Particle> fluidParticles, SpatialHash spatialHash)
    {
        foreach (var bp in BoundaryParticles)
        {
            double sumWeight = 0.0;
            double sumWeightedPressure = 0.0;
            Vector3 sumWeightedVelocity = Vector3.Zero;
            double sumDensity = 0.0;
            
            // Query nearby fluid particles
            var neighbors = spatialHash.FindNeighbors(bp.Position, _kernelRadius);
            
            foreach (var p in neighbors)
            {
                if (p.IsBoundary) continue;
                
                Vector3 r = bp.Position - p.Position;
                double dist = r.Length;
                
                if (dist > _kernelRadius || dist < 1e-6) continue;
                
                double W = KernelFunctions.CubicSpline(dist, _smoothingLength);
                
                sumWeight += W;
                sumWeightedPressure += p.Pressure * W;
                sumWeightedVelocity += p.Velocity * W;
                sumDensity += p.Mass * W;
            }
            
            // Normalize
            if (sumWeight > 1e-10)
            {
                bp.Pressure = sumWeightedPressure / sumWeight;
                bp.FluidVelocity = sumWeightedVelocity / sumWeight;
            }
            else
            {
                bp.Pressure = 0.0;
                bp.FluidVelocity = Vector3.Zero;
            }
            
            bp.Density = sumDensity > 0 ? sumDensity : 1000.0;
        }
    }
    
    /// <summary>
    /// Step 2: Calculate fluid forces on rigid body from boundary particles
    /// 
    /// Pressure force: F_pressure = -Σ_j (p_j * A_j * n_j)
    /// Drag force: F_drag = 0.5 * C_d * ρ * A * |u_rel| * u_rel
    /// 
    /// where u_rel = u_fluid - u_boundary
    /// 
    /// Returns (total force, total torque about body center)
    /// </summary>
    public (Vector3 force, Vector3 torque) CalculateFluidForces(RigidBody body)
    {
        Vector3 totalForce = Vector3.Zero;
        Vector3 totalTorque = Vector3.Zero;
        
        foreach (var bp in BoundaryParticles)
        {
            // Pressure force: F = -p * A * n (acts inward on fluid, outward on body)
            Vector3 pressureForce = -bp.Pressure * bp.Area * bp.Normal;
            
            // Drag force: F_drag = 0.5 * C_d * ρ * A * |u_rel| * u_rel
            // Note: For submerged surfaces only (check if below free surface)
            Vector3 relativeVelocity = bp.FluidVelocity - bp.Velocity;
            double relSpeed = relativeVelocity.Length;
            
            Vector3 dragForce = Vector3.Zero;
            if (relSpeed > 1e-6 && bp.Density > 100.0) // Only if there's fluid nearby
            {
                // Simplified: use full area (should project onto normal component)
                dragForce = 0.5 * _dragCoefficient * bp.Density * bp.Area * relSpeed * relativeVelocity;
            }
            
            Vector3 totalBoundaryForce = pressureForce + dragForce;
            
            totalForce += totalBoundaryForce;
            
            // Torque = r × F (lever arm from body center)
            Vector3 leverArm = bp.Position - body.Position;
            totalTorque += Vector3.Cross(leverArm, totalBoundaryForce);
        }
        
        return (totalForce, totalTorque);
    }
    
    /// <summary>
    /// Step 3: Apply reaction forces to fluid particles (Newton's 3rd law)
    /// 
    /// For each boundary particle with force F_b acting on the body,
    /// distribute reaction force -F_b to nearby SPH particles using kernel weighting:
    /// 
    /// ΔF_i = -F_b * (m_i * W(r_i, h)) / Σ_j (m_j * W(r_j, h))
    /// 
    /// This ensures momentum conservation: Σ ΔF_i = -F_b
    /// </summary>
    public void ApplyReactionForcesToFluid(
        RigidBody body, 
        IReadOnlyList<Particle> fluidParticles,
        SpatialHash spatialHash)
    {
        foreach (var bp in BoundaryParticles)
        {
            // Compute force on this boundary element
            Vector3 pressureForce = -bp.Pressure * bp.Area * bp.Normal;
            
            Vector3 relativeVelocity = bp.FluidVelocity - bp.Velocity;
            double relSpeed = relativeVelocity.Length;
            
            Vector3 dragForce = Vector3.Zero;
            if (relSpeed > 1e-6 && bp.Density > 100.0)
            {
                dragForce = 0.5 * _dragCoefficient * bp.Density * bp.Area * relSpeed * relativeVelocity;
            }
            
            Vector3 boundaryForce = pressureForce + dragForce;
            Vector3 reactionForce = -boundaryForce; // Newton's 3rd law
            
            // Find nearby fluid particles and compute kernel weight sum
            var neighbors = spatialHash.FindNeighbors(bp.Position, _kernelRadius);
            
            double sumWeightedMass = 0.0;
            foreach (var p in neighbors)
            {
                if (p.IsBoundary) continue;
                
                Vector3 r = bp.Position - p.Position;
                double dist = r.Length;
                
                if (dist > _kernelRadius || dist < 1e-6) continue;
                
                double W = KernelFunctions.CubicSpline(dist, _smoothingLength);
                sumWeightedMass += p.Mass * W;
            }
            
            // Distribute reaction force proportional to kernel weight
            if (sumWeightedMass > 1e-10)
            {
                foreach (var p in neighbors)
                {
                    if (p.IsBoundary) continue;
                    
                    Vector3 r = bp.Position - p.Position;
                    double dist = r.Length;
                    
                    if (dist > _kernelRadius || dist < 1e-6) continue;
                    
                    double W = KernelFunctions.CubicSpline(dist, _smoothingLength);
                    double distributionFactor = (p.Mass * W) / sumWeightedMass;
                    
                    // Apply distributed force as acceleration: a = F/m
                    Vector3 acceleration = (reactionForce * distributionFactor) / p.Mass;
                    p.Acceleration += acceleration;
                }
            }
        }
    }
}
