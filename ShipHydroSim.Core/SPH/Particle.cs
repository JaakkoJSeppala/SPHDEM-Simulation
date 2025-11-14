using ShipHydroSim.Core.Geometry;

namespace ShipHydroSim.Core.SPH;

/// <summary>
/// SPH particle with physical properties
/// </summary>
public class Particle
{
    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Acceleration { get; set; }
    
    public double Mass { get; set; }
    public double Density { get; set; }
    public double Pressure { get; set; }
    
    // For visualization/debugging
    public bool IsBoundary { get; set; }

    public Particle(int id, Vector3 position, double mass = 1.0)
    {
        Id = id;
        Position = position;
        Velocity = Vector3.Zero;
        Acceleration = Vector3.Zero;
        Mass = mass;
        Density = 1000.0; // water density kg/m³
        Pressure = 0.0;
        IsBoundary = false;
    }
}
