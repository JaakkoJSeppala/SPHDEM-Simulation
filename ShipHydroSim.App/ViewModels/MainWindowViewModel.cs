using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShipHydroSim.App.Models;
using System;
using System.Timers;

namespace ShipHydroSim.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SimulationEngine _engine;
    private readonly Timer _uiUpdateTimer;
    
    [ObservableProperty]
    private string _particleCount = "0";
    
    [ObservableProperty]
    private string _simulationTime = "0.00";
    
    [ObservableProperty]
    private string _averageHeight = "0.00";
    
    [ObservableProperty]
    private string _averageVelocity = "0.00";
    
    [ObservableProperty]
    private bool _isRunning = false;
    
    [ObservableProperty]
    private double _smoothingLength = 0.2;
    
    [ObservableProperty]
    private double _restDensity = 1000.0;
    
    [ObservableProperty]
    private double _stiffness = 20000.0;
    
    [ObservableProperty]
    private double _viscosity = 0.05;
    
    [ObservableProperty]
    private bool _enableWaves = false;
    
    [ObservableProperty]
    private string _selectedWaveType = "Sine";
    
    [ObservableProperty]
    private string _shipRoll = "0.00";
    
    [ObservableProperty]
    private string _shipPitch = "0.00";
    
    [ObservableProperty]
    private string _shipHeave = "0.00";
    
    [ObservableProperty]
    private int _maxParticles = 3000; // Reduced from 5000 for better performance
    
    [ObservableProperty]
    private double _targetFps = 30;
    
    [ObservableProperty]
    private bool _adaptiveTimeStep = true;
    
    public string[] WaveTypes { get; } = { "None", "Sine", "Stokes", "Irregular" };
    
    public SimulationEngine Engine => _engine;
    
    public MainWindowViewModel()
    {
        _engine = new SimulationEngine();
        _engine.InitializeDamBreakWithShip();
        
        // Enable waves by default
        EnableWaves = true;
        SelectedWaveType = "Sine";
        
        // Update UI statistics periodically
        _uiUpdateTimer = new Timer(100); // 10 Hz
        _uiUpdateTimer.Elapsed += (s, e) => UpdateStatistics();
        _uiUpdateTimer.Start();
        
        UpdateStatistics();
    }
    
    [RelayCommand]
    private void Start()
    {
        if (!IsRunning)
        {
            System.Console.WriteLine("Starting simulation...");
            
            // Apply performance settings
            if (_engine.Solver is ShipHydroSim.Core.Hybrid.HybridSolver solver)
            {
                solver.MaxParticles = MaxParticles;
                solver.TargetFPS = TargetFps;
                solver.AdaptiveTimeStep = AdaptiveTimeStep;
                
                System.Console.WriteLine($"Waves enabled: {EnableWaves}, Type: {SelectedWaveType}");
                
                // Enable waves based on selection
                solver.EnableWaves = EnableWaves;
                if (EnableWaves)
                {
                    solver.WaveGenerator = SelectedWaveType switch
                    {
                        "Sine" => new ShipHydroSim.Core.Waves.SineWave 
                        { 
                            Amplitude = 0.4,      // Suurempi amplitudi → näkyvämmät aallot
                            WaveLength = 4.0,     // Pidempi aalto
                            Period = 2.0,         // Hitaampi aalto
                            Direction = new ShipHydroSim.Core.Geometry.Vector3(1, 0, 0)
                        },
                        "Stokes" => new ShipHydroSim.Core.Waves.StokesWave 
                        { 
                            Amplitude = 0.5,      // Vielä suurempi epälineaarisessa
                            WaveLength = 4.0, 
                            Period = 2.0 
                        },
                        "Irregular" => new ShipHydroSim.Core.Waves.IrregularWave(
                            significantWaveHeight: 0.6,  // Merkitsevä aallonkorkeus
                            peakPeriod: 2.5, 
                            numComponents: 15
                        ),
                        _ => null
                    };
                    
                    System.Console.WriteLine($"Wave generator created: {solver.WaveGenerator?.GetType().Name}");
                }
            }
            
            _engine.Start();
            IsRunning = true;
            System.Console.WriteLine("Simulation started!");
        }
    }
    
    [RelayCommand]
    private void Pause()
    {
        if (IsRunning)
        {
            _engine.Pause();
            IsRunning = false;
        }
    }
    
    [RelayCommand]
    private void Reset()
    {
        _engine.Reset();
        IsRunning = false;
        UpdateStatistics();
    }
    
    [RelayCommand]
    private void ApplyParameters()
    {
        _engine.Solver.SmoothingLength = SmoothingLength;
        _engine.Solver.RestDensity = RestDensity;
        _engine.Solver.Stiffness = Stiffness;
        _engine.Solver.Viscosity = Viscosity;
    }
    
    [RelayCommand]
    private void ApplyWaves()
    {
        if (SelectedWaveType == "None")
        {
            _engine.Solver.EnableWaves = false;
        }
        else
        {
            _engine.InitializeWaveScenario(SelectedWaveType);
        }
    }
    
    private void UpdateStatistics()
    {
        var particles = _engine.Solver.Particles;
        ParticleCount = particles.Count.ToString();
        SimulationTime = $"{_engine.CurrentTime:F2}";
        
        // Compute average height and velocity
        double sumHeight = 0;
        double sumVel = 0;
        int count = 0;
        
        foreach (var p in _engine.Solver.Particles)
        {
            if (!p.IsBoundary)
            {
                sumHeight += p.Position.Y;
                sumVel += p.Velocity.Length;
                count++;
            }
        }
        
        if (count > 0)
        {
            AverageHeight = $"{sumHeight / count:F2}";
            AverageVelocity = $"{sumVel / count:F2}";
        }
        
        // Ship motion (6DOF)
        if (_engine.Solver.RigidBodies.Count > 0 && _engine.Solver.RigidBodies[0] is Core.Ships.ShipRigidBody ship)
        {
            ShipRoll = $"{ship.Roll * 180 / Math.PI:F2}°";
            ShipPitch = $"{ship.Pitch * 180 / Math.PI:F2}°";
            ShipHeave = $"{ship.Position.Y - 2.0:F2} m";
        }
    }
}
