using System.Reflection;
using System.Runtime.Loader;
using ShipHydroSim.PluginAPI;
using ShipHydroSim.Core;
using ShipHydroSim.Core.SPH;
using ShipHydroSim.Core.DEM;

namespace ShipHydroSim.PluginHost;

public class PluginLoader : IShipSimHost
{
    public List<IShipSimPlugin> Plugins { get; } = new();
    
    public ISimulationSolver? CurrentSolver { get; set; }
    public SimulationParameters Parameters { get; set; } = new();

    private readonly List<Particle> _particles = new();
    private readonly List<RigidBody> _rigidBodies = new();

    public void LoadPlugins(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (var dll in Directory.GetFiles(directory, "*.dll"))
        {
            var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dll));
            var types = asm.GetTypes()
                .Where(t => !t.IsAbstract && typeof(IShipSimPlugin).IsAssignableFrom(t));

            foreach (var t in types)
            {
                if (Activator.CreateInstance(t) is IShipSimPlugin plugin)
                {
                    plugin.Initialize(this);
                    Plugins.Add(plugin);
                }
            }
        }
    }

    public void Log(string message)
    {
        Console.WriteLine($"[PLUGIN] {message}");
    }

    public void AddParticle(Particle particle)
    {
        _particles.Add(particle);
    }

    public void AddRigidBody(RigidBody body)
    {
        _rigidBodies.Add(body);
    }
}
