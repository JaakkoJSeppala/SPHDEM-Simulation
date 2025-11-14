namespace ShipHydroSim.PluginAPI;

public interface IShipSimPlugin
{
    string Name { get; }
    Version Version { get; }

    void Initialize(IShipSimHost host);
    void OnSimulationStart();
    void OnSimulationStep(double dt);
    void OnSimulationEnd();
}

/// <summary>
/// Plugin that provides custom force models for SPH
/// </summary>
public interface IForceModelPlugin : IShipSimPlugin
{
    void ComputeForces();
}

/// <summary>
/// Plugin that provides custom integrators
/// </summary>
public interface IIntegratorPlugin : IShipSimPlugin
{
    void Integrate(double dt);
}
