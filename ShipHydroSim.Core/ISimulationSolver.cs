using ShipHydroSim.Core.Geometry;

namespace ShipHydroSim.Core;

/// <summary>
/// Base interface for simulation solvers
/// </summary>
public interface ISimulationSolver
{
    void Step();
    double TimeStep { get; set; }
    int ParticleCount { get; }
}

/// <summary>
/// Simulation parameters for SPH + DEM coupling
/// </summary>
public class SimulationParameters
{
    public double TimeStep { get; set; } = 0.001;
    public double SimulationTime { get; set; } = 0.0;
    public int StepCount { get; set; } = 0;
    
    // Domain
    public Vector3 DomainMin { get; set; } = Vector3.Zero;
    public Vector3 DomainMax { get; set; } = new(10, 10, 10);
    
    // SPH parameters
    public double SmoothingLength { get; set; } = 0.1;
    public double RestDensity { get; set; } = 1000.0;
    public double Stiffness { get; set; } = 10000.0;
    public double Viscosity { get; set; } = 0.01;
    
    // DEM parameters
    public double ContactStiffness { get; set; } = 1e6;
    public double ContactDamping { get; set; } = 100.0;
    public double FrictionCoefficient { get; set; } = 0.3;
    
    // Environment
    public Vector3 Gravity { get; set; } = new(0, -9.81, 0);
}
