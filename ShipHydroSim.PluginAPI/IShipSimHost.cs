using ShipHydroSim.Core;
using ShipHydroSim.Core.SPH;
using ShipHydroSim.Core.DEM;

namespace ShipHydroSim.PluginAPI;

public interface IShipSimHost
{
    void Log(string message);
    
    // Access to simulation state
    ISimulationSolver? CurrentSolver { get; }
    SimulationParameters Parameters { get; }
    
    // Particle and rigid body management
    void AddParticle(Particle particle);
    void AddRigidBody(RigidBody body);
}
