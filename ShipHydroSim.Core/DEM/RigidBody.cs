using ShipHydroSim.Core.Geometry;

namespace ShipHydroSim.Core.DEM;

/// <summary>
/// Rigid body for DEM simulation (e.g., ship hull)
/// </summary>
public class RigidBody
{
    public int Id { get; set; }
    
    // Linear motion
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Force { get; set; }
    
    // Angular motion
    public Quaternion Orientation { get; set; }
    public Vector3 AngularVelocity { get; set; }
    public Vector3 Torque { get; set; }
    
    // Physical properties
    public double Mass { get; set; }
    public double InverseMass { get; set; }
    public Matrix3x3 InertiaTensor { get; set; }
    public Matrix3x3 InverseInertiaTensor { get; set; }
    
    // Geometry reference (will be implemented later with meshes)
    public IShape Shape { get; set; }
    
    public bool IsStatic { get; set; }

    public RigidBody(int id, Vector3 position, double mass, IShape shape)
    {
        Id = id;
        Position = position;
        Velocity = Vector3.Zero;
        Force = Vector3.Zero;
        
        Orientation = Quaternion.Identity;
        AngularVelocity = Vector3.Zero;
        Torque = Vector3.Zero;
        
        Mass = mass;
        InverseMass = mass > 0 ? 1.0 / mass : 0.0;
        Shape = shape;
        
        // Default inertia tensor (sphere-like)
        double I = 0.4 * mass * 1.0 * 1.0; // Assuming radius = 1
        InertiaTensor = Matrix3x3.Diagonal(I, I, I);
        InverseInertiaTensor = Matrix3x3.Diagonal(1.0 / I, 1.0 / I, 1.0 / I);
        
        IsStatic = false;
    }
}

/// <summary>
/// Base interface for collision shapes
/// </summary>
public interface IShape
{
    double GetBoundingRadius();
}

/// <summary>
/// Simple sphere shape for initial testing
/// </summary>
public class SphereShape : IShape
{
    public double Radius { get; set; }
    
    public SphereShape(double radius)
    {
        Radius = radius;
    }
    
    public double GetBoundingRadius() => Radius;
}
