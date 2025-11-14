using ShipHydroSim.Core.Geometry;

namespace ShipHydroSim.Core.Coupling;

/// <summary>
/// Virtual SPH boundary particle on rigid body surface
/// Represents a surface element with position, normal, and area
/// Used for computing fluid-structure interaction forces
/// 
/// Reference: Adami et al. (2012) "A generalized wall boundary condition for SPH"
/// </summary>
public class BoundaryParticle
{
    /// <summary>Position in world space (updated each timestep)</summary>
    public Vector3 Position { get; set; }
    
    /// <summary>Surface normal pointing outward from solid (unit vector)</summary>
    public Vector3 Normal { get; set; }
    
    /// <summary>Associated surface area (for pressure force integration)</summary>
    public double Area { get; set; }
    
    /// <summary>Position in body-local coordinates (constant)</summary>
    public Vector3 LocalPosition { get; set; }
    
    /// <summary>Normal in body-local coordinates (constant)</summary>
    public Vector3 LocalNormal { get; set; }
    
    /// <summary>Velocity of this boundary point (computed from rigid body motion)</summary>
    public Vector3 Velocity { get; set; }
    
    /// <summary>SPH pressure extrapolated at this boundary location</summary>
    public double Pressure { get; set; }
    
    /// <summary>SPH density at this boundary location</summary>
    public double Density { get; set; }
    
    /// <summary>Average fluid velocity near this boundary (kernel-weighted)</summary>
    public Vector3 FluidVelocity { get; set; }
    
    public BoundaryParticle(Vector3 localPos, Vector3 localNormal, double area)
    {
        LocalPosition = localPos;
        LocalNormal = localNormal.Normalized();
        Area = area;
        Position = localPos;
        Normal = localNormal.Normalized();
        Density = 1000.0; // Initialize to water density
    }
    
    /// <summary>
    /// Update world-space position and normal from rigid body transform
    /// </summary>
    public void UpdateTransform(Vector3 bodyPosition, Quaternion bodyOrientation)
    {
        // Transform position: x_world = R * x_local + t
        Position = bodyOrientation.Rotate(LocalPosition) + bodyPosition;
        
        // Transform normal: n_world = R * n_local
        Normal = bodyOrientation.Rotate(LocalNormal).Normalized();
    }
    
    /// <summary>
    /// Update velocity from rigid body motion: v = v_cm + ω × r
    /// </summary>
    public void UpdateVelocity(Vector3 bodyVelocity, Vector3 bodyAngularVelocity, Vector3 bodyPosition)
    {
        Vector3 r = Position - bodyPosition;
        Velocity = bodyVelocity + Vector3.Cross(bodyAngularVelocity, r);
    }
}
