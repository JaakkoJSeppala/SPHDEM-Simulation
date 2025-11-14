# ShipHydroSim

**3D Ship Hydrodynamics Simulator: SPH + DEM Hybrid Method**

A computational science Master's thesis project implementing a hybrid simulator combining:
- **SPH (Smooth Particle Hydrodynamics)**: Fluid mechanics particle simulation
- **DEM (Discrete Element Method)**: Rigid body dynamics (6DOF)
- **Wave Modeling**: Sine, Stokes, and JONSWAP spectrum waves
- **Fluid-Structure Coupling**: Two-way SPH ↔ ship interaction
- **Avalonia GUI**: Real-time 3D visualization

## 📁 Project Structure

```
ShipHydroSim/
├── ShipHydroSim.Core/           # Core: physics engine
│   ├── Geometry/                # Vector3, Quaternion, Matrix3x3
│   ├── SPH/                     # Particles, kernel functions, SPHSolver
│   ├── DEM/                     # RigidBody, collision shapes
│   ├── Ships/                   # ShipRigidBody (6DOF, buoyancy, hydrostatics)
│   ├── Waves/                   # WaveGenerator (Sine, Stokes, Irregular/JONSWAP)
│   ├── Coupling/                # BoundaryForceCalculator (SPH ↔ DEM)
│   ├── Hybrid/                  # HybridSolver (integrated SPH+DEM+Waves)
│   ├── Spatial/                 # SpatialHash (neighbor search O(1))
│   └── HeightField.cs           # 2D height field
│
├── ShipHydroSim.App/            # Avalonia GUI (cross-platform)
│   ├── Models/                  # SimulationEngine
│   ├── ViewModels/              # MVVM (MainWindowViewModel)
│   ├── Views/                   # MainWindow (UI controls)
│   ├── Controls/                # ParticleViewport (Skia 3D rendering)
│   └── Rendering/               # Custom draw operations
│
├── ShipHydroSim.PluginAPI/      # Plugin interfaces
├── ShipHydroSim.PluginHost/     # Plugin loader
├── ShipHydroSim.Demo/           # Console demo
└── Example.WavePlugin/          # Example plugin
```

## 🚀 Getting Started

### Requirements
- .NET 10.0 SDK
- Windows/Linux/macOS (cross-platform)
- GPU not required (CPU-based, optimized)

### Building
```powershell
git clone https://github.com/JaakkoJSeppala/SPHDEM-Simulation.git
cd SPHDEM-Simulation
dotnet build ShipHydroSim.sln -c Release
```

### Running the GUI Application
```powershell
cd ShipHydroSim.App
dotnet run -c Release
```

**Usage:**
1. Select scenario: "Dam Break" or "Wave Scenario"
2. If "Wave Scenario", choose wave type (Sine/Stokes/Irregular)
3. Press **Start** to begin simulation
4. Use mouse to rotate camera in viewport
5. Monitor statistics: particle count, FPS, ship motion (roll/pitch/heave)

### Running Validation Tests
```powershell
# Fast analytical tests (< 1 second)
dotnet run --project ShipHydroSim.ValidationRunner

# Generate thesis materials (LaTeX/CSV/Python)
dotnet run --project ShipHydroSim.ValidationRunner -- --thesis

# Full simulation tests (5-10 minutes)
dotnet run --project ShipHydroSim.ValidationRunner -- --full
```

**Validation tests verify:**
- ✅ Kernel normalization (∫W dV = 1)
- ✅ Hydrostatic pressure (p = ρgh)
- ✅ Archimedes principle
- ✅ Terminal velocity (Stokes)
- ✅ Wave dispersion relation
- ✅ Newton's 3rd law (momentum conservation)
- ✅ Drag force formula
- ✅ Quaternion mathematics

Full simulation tests compare against:
- Martin & Moyce (1952) dam break experiments
- Analytical terminal velocity
- Hydrostatic equilibrium theory

**Thesis materials:**  
The `--thesis` flag generates ready-to-use thesis materials in `ThesisValidation/`:
- `validation_section.tex` - LaTeX section with tables and equations
- `validation_results.csv` - Data for Excel/analysis
- `plot_validation.py` - Python script for publication figures
- `validation_summary.txt` - Text summary

See `ThesisValidation/README.md` for integration guide.

## 🧪 Implemented Features

### SPH (Smooth Particle Hydrodynamics)
- **Kernel functions**: Cubic Spline (M4 B-spline)
- **Density**: ρᵢ = Σⱼ mⱼ W(rᵢ - rⱼ, h)
- **Pressure**: Choice of Tait EOS (p = k((ρ/ρ₀)^γ - 1)) or linear (p = c²(ρ - ρ₀))
- **Forces**: Pressure (symmetric SPH), viscosity (XSPH), surface tension (optional)
- **Neighbor search**: Spatial Hash (O(1) insertion, O(N_local) query)
- **Integration**: Explicit Euler, adaptive timestep (CFL + performance)
- **Boundary conditions**: Reflective tank walls (configurable domain boundaries)

### DEM (Discrete Element Method)
- **6DOF Rigid Body**: Position, velocity, orientation (quaternion)
- **Inertia tensor**: Box-approximation for ship hull
- **Angular motion**: Correct body-to-world inertia transform
- **Damping**: Hydrodynamic damping coefficients

### Ship Hydrodynamics
- **Buoyancy**: Archimedes principle, block coefficient (Cb)
- **Hydrostatic restoring moments**: Metacentric heights GM_roll, GM_pitch
- **Differential buoyancy**: 4-corner wave sampling → roll/pitch excitation
- **Wave forcing**: Gentle vertical + horizontal coupling with depth decay
- **Motion tracking**: Roll, pitch, heave extraction from quaternion

### Wave Models
- **SineWave**: Linear sinusoidal waves
- **StokesWave**: 2nd-order Stokes expansion (nonlinear)
- **IrregularWave**: JONSWAP spectrum, multi-component superposition

### SPH ↔ DEM Coupling
- **Fluid → Ship**: Pressure-based forces, viscous drag
- **Ship → Fluid**: Velocity blending near ship boundary
- **Two-way coupling**: Momentum exchange

### Performance Optimizations
- **Fast EOS**: Linear equation of state (no `Math.Pow`)
- **Configurable neighbor radius**: Trade accuracy for speed
- **Surface tension toggle**: Disable for CPU savings
- **Adaptive timestep**: Performance-based + CFL-based clamping
- **Particle mass**: Consistent ρ·ΔV³ to match spacing
- **Release build optimized**: ~30-60 FPS with 5k-15k particles

### GUI (Avalonia + Skia)
- **3D Viewport**: Custom Skia draw operations for particles
- **Ship rendering**: Quaternion-based wireframe rotation, dynamic coloring
- **Camera control**: Mouse drag to rotate view
- **Real-time stats**: Particle count, FPS, simulation time, roll/pitch/heave
- **Scenario selection**: Dam break, wave scenarios (Sine/Stokes/Irregular)
- **Start/Pause/Reset controls**

## ⚙️ Parameters & Tuning

### Performance
- `MaxParticles`: 10000 (memory limit)
- `TargetFPS`: 30-45 (adaptive throttling)
- `AdaptiveTimeStep`: true (performance + CFL)
- `UseLinearEOS`: true (faster than Tait)
- `EnableSurfaceTension`: false (CPU savings)
- `NeighborRadiusFactor`: 2.0 (reduce to 1.8 for speed)

### SPH Parameters
- `SmoothingLength`: 0.16-0.25 (1.2-1.5 × spacing)
- `RestDensity`: 1000 kg/m³
- `Stiffness`: 20000 Pa (if using Tait)
- `SoundSpeed`: 25 m/s (if using linear EOS)
- `Viscosity`: 0.05 Pa·s

### Ship Parameters
- `GM_Roll`: 0.5 m (metacentric height, roll stability)
- `GM_Pitch`: 0.3 m (pitch stability)
- `DampingCoeff`: 0.1 (hydrodynamic damping)
- `BlockCoefficient`: 0.7 (hull fullness, mass/buoyancy)

### Waves
- **Sine**: Amplitude 0.3m, λ=4m, T=2s
- **Stokes**: Amplitude 0.4m, λ=4m, T=2s
- **Irregular**: Hs=0.5m, Tp=2.5s, 20 components

## 🔌 Plugin Architecture

Create your own plugin by implementing `IShipSimPlugin`:

```csharp
public class MyPlugin : IShipSimPlugin
{
    public string Name => "MyPlugin";
    public Version Version => new(1, 0, 0);

    public void Initialize(IShipSimHost host)
    {
        host.Log("MyPlugin initialized!");
    }

    public void OnSimulationStart() { }
    public void OnSimulationStep(double dt) { }
    public void OnSimulationEnd() { }
}
```

Plugins are loaded dynamically from the `Plugins/` folder.

## 📊 Future Work

### Near-term Improvements
- [ ] UI sliders for parameters (GM, damping, wave amplitude)
- [ ] Time-series plotting (roll/pitch/heave history)
- [ ] CSV export of simulation data
- [ ] Mesh-based ship geometry (STL/OBJ import)

### Physics Extensions
- [ ] PCISPH (Predictive-Corrective, better incompressibility)
- [ ] Verlet/Leapfrog integrator (energy conservation)
- [ ] Turbulence models (k-ε, LES)
- [ ] Added mass terms
- [ ] Wave radiation damping

### Performance
- [ ] GPU acceleration (CUDA/OpenCL/Compute Shaders)
- [ ] Parallel.For in SPH loops
- [ ] Adaptive smoothing length
- [ ] Multi-level spatial hash

### Validation & Benchmarks
- [ ] Dam break: comparison to experimental data
- [ ] Sloshing tank: natural frequency analysis
- [ ] Ship roll decay: damping coefficient estimation
- [ ] Convergence tests (particle spacing, timestep)

## 📚 References

### SPH Theory & Methods
- Monaghan (1992): "Smoothed Particle Hydrodynamics"
- Monaghan (2005): "Smoothed particle hydrodynamics" (review)
- Ihmsen et al. (2014): "SPH Fluids in Computer Graphics"
- Becker & Teschner (2007): "Weakly compressible SPH"

### Ship Hydrodynamics
- Faltinsen (1990): "Sea Loads on Ships and Offshore Structures"
- Newman (1977): "Marine Hydrodynamics"
- Journée & Massie (2001): "Offshore Hydromechanics"

### SPH-DEM Coupling
- Robinson et al. (2014): "Fluid-particle flow simulations using two-way-coupled mesoscale SPH-DEM"
- Canelas et al. (2016): "SPH-DCDEM model for arbitrary geometries in free surface solid-fluid flows"

### Wave Modeling
- Dean & Dalrymple (1991): "Water Wave Mechanics"
- Hasselmann et al. (1973): "JONSWAP spectrum measurements"

## 🐛 Known Limitations

- Currently box-based ship geometry only (mesh support coming)
- Collisions only between ship and fluid (no ship-ship)
- CPU-based (GPU acceleration in development)
- Spatial hash fixed cell size (adaptive coming)
- No periodic boundary conditions (reflective walls only)

## 🤝 Contributing

This is an academic research project. To suggest improvements:
1. Open an issue on GitHub
2. Fork the repository and create a pull request
3. Ensure code builds (`dotnet build`)
4. Add comments/documentation for changes

## 📝 License

This project was developed for research purposes as part of a Master's thesis at the University of Jyväskylä.

## 👤 Author

**Jaakko Seppälä**  
Computational Science, University of Jyväskylä  
Master's Thesis: "3D Ship Hydrodynamics using SPH-DEM Coupling"  
GitHub: [@JaakkoJSeppala](https://github.com/JaakkoJSeppala)

---

**Version**: 1.0.0 (November 2025)  
**Status**: Active development  
**Target**: M.Sc. Thesis (Computational Science)
