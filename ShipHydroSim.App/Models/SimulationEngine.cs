using System;
using System.Threading;
using System.Threading.Tasks;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.SPH;
using ShipHydroSim.Core.Hybrid;
using ShipHydroSim.Core.Ships;
using ShipHydroSim.Core.Waves;

namespace ShipHydroSim.App.Models;

/// <summary>
/// Simulation engine that runs hybrid SPH+DEM solver in background
/// </summary>
public class SimulationEngine
{
    private readonly HybridSolver _solver;
    private CancellationTokenSource? _cts;
    private Task? _simulationTask;
    
    public HybridSolver Solver => _solver;
    public bool IsRunning { get; private set; }
    public double CurrentTime => _solver.CurrentTime;
    public int StepCount => _solver.StepCount;
    
    // Events for UI updates
    public event Action? OnSimulationStep;
    
    public SimulationEngine()
    {
        _solver = new HybridSolver
        {
            SmoothingLength = 0.2,
            RestDensity = 1000.0,
            Stiffness = 20000.0,
            Viscosity = 0.05,
            Gravity = new Vector3(0, -9.81, 0),
            TimeStep = 0.002 // Larger time step for faster computation (was 0.001)
        };
    }
    
    public void InitializeDamBreakWithShip()
    {
        // Clear existing (CurrentTime and StepCount are read-only from Solver)
        
        // Create water body: 10m x 2m deep x 10m
        int particleId = 0;
        double spacing = 0.25; // particle spacing (m)
        // Ensure SPH kernel matches spacing scale
        _solver.SmoothingLength = Math.Max(0.18, spacing * 1.2);
        double particleMass = _solver.RestDensity * spacing * spacing * spacing; // ρ * ΔV
        
        // Water fills the bottom area
        for (double x = -5.0; x <= 5.0; x += spacing)
        {
            for (double y = 0.0; y <= 2.0; y += spacing) // Water 2m deep
            {
                for (double z = -5.0; z <= 5.0; z += spacing)
                {
                    var particle = new Particle(particleId++, new Vector3(x, y, z), mass: particleMass)
                    {
                        Velocity = Vector3.Zero
                    };
                    _solver.AddParticle(particle);
                }
            }
        }
        
        // Place ship slightly deeper so corners experience wave variation
        var ship = new ShipRigidBody(0, new Vector3(0, 1.9, 0), length: 3.0, beam: 1.0, draft: 0.5);
        _solver.AddRigidBody(ship);
        _solver.BaseWaterLevel = 2.0; // Mean surface for this scenario
        // Set tank boundaries for this scenario
        _solver.DomainMin = new Vector3(-5.0, 0.0, -5.0);
        _solver.DomainMax = new Vector3(5.0, 4.0, 5.0); // ceiling above splash cap
        _solver.MaxWaterHeight = 4.0;
    }
    
    public void InitializeWaveScenario(string waveType)
    {
        // Clear and create wave pool: 20m x 3m deep x 20m
        int particleId = 0;
        double spacing = 0.20;
        _solver.SmoothingLength = Math.Max(0.16, spacing * 1.2);
        double particleMass = _solver.RestDensity * spacing * spacing * spacing;
        
        for (double x = -10.0; x <= 10.0; x += spacing)
        {
            for (double y = 0.0; y <= 3.0; y += spacing) // Water 3m deep
            {
                for (double z = -10.0; z <= 10.0; z += spacing)
                {
                    var particle = new Particle(particleId++, new Vector3(x, y, z), mass: particleMass)
                    {
                        Velocity = Vector3.Zero
                    };
                    _solver.AddParticle(particle);
                }
            }
        }
        
        // Place ship slightly submerged for wave excitation
        var ship = new ShipRigidBody(0, new Vector3(0, 2.8, 0), length: 3.0, beam: 1.0, draft: 0.5);
        _solver.AddRigidBody(ship);
        _solver.BaseWaterLevel = 3.0; // Mean surface for wave pool
        // Set wider pool boundaries
        _solver.DomainMin = new Vector3(-10.0, 0.0, -10.0);
        _solver.DomainMax = new Vector3(10.0, 5.0, 10.0);
        _solver.MaxWaterHeight = 5.0;
        
        // Setup wave generator
        _solver.EnableWaves = true;
        _solver.WaveGenerator = waveType switch
        {
            "Sine" => new SineWave { Amplitude = 0.3, WaveLength = 4.0, Period = 2.0 },
            "Stokes" => new StokesWave { Amplitude = 0.4, WaveLength = 4.0, Period = 2.0 },
            "Irregular" => new IrregularWave(significantWaveHeight: 0.5, peakPeriod: 2.5, numComponents: 20),
            _ => null
        };
    }
    
    public void Start()
    {
        if (IsRunning) return;
        
        IsRunning = true;
        _cts = new CancellationTokenSource();
        
        _simulationTask = Task.Run(() => SimulationLoop(_cts.Token));
    }
    
    public void Pause()
    {
        IsRunning = false;
        _cts?.Cancel();
        _simulationTask?.Wait(100);
    }
    
    public void Reset()
    {
        Pause();
        InitializeDamBreakWithShip();
    }
    
    private void SimulationLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _solver.Step();
            
            OnSimulationStep?.Invoke();
            
            // Limit update rate to ~120 FPS (faster, was ~60)
            Thread.Sleep(8);
        }
    }
}
