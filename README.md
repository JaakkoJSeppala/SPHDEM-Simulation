# ShipHydroSim

**3D Ship Hydrodynamics Simulator: SPH + DEM Hybrid Method**

Tämä projekti on laskennallisen tieteen gradu-työtä varten kehitetty simulaattori, joka yhdistää:
- **SPH (Smooth Particle Hydrodynamics)**: Nestemekaniikan partikkelisimulaatio
- **DEM (Discrete Element Method)**: Jäykkien kappaleiden dynamiikka (6DOF)
- **Wave Modeling**: Sine, Stokes, ja JONSWAP-spektri aallot
- **Fluid-Structure Coupling**: Kahdensuuntainen SPH ↔ laiva vuorovaikutus
- **Avalonia GUI**: Reaaliaikainen 3D-visualisointi

## 📁 Projektirakenne

```
ShipHydroSim/
├── ShipHydroSim.Core/           # Ydin: fysiikka-moottori
│   ├── Geometry/                # Vector3, Quaternion, Matrix3x3
│   ├── SPH/                     # Partikkelit, kernel-funktiot, SPHSolver
│   ├── DEM/                     # RigidBody, collision shapes
│   ├── Ships/                   # ShipRigidBody (6DOF, buoyancy, hydrostatics)
│   ├── Waves/                   # WaveGenerator (Sine, Stokes, Irregular/JONSWAP)
│   ├── Coupling/                # BoundaryForceCalculator (SPH ↔ DEM)
│   ├── Hybrid/                  # HybridSolver (integroitu SPH+DEM+Waves)
│   ├── Spatial/                 # SpatialHash (naapurihaku O(1))
│   └── HeightField.cs           # 2D pinnankorkeuskenttä
│
├── ShipHydroSim.App/            # Avalonia GUI (cross-platform)
│   ├── Models/                  # SimulationEngine
│   ├── ViewModels/              # MVVM (MainWindowViewModel)
│   ├── Views/                   # MainWindow (UI controls)
│   ├── Controls/                # ParticleViewport (Skia 3D rendering)
│   └── Rendering/               # Custom draw operations
│
├── ShipHydroSim.PluginAPI/      # Plugin-rajapinnat
├── ShipHydroSim.PluginHost/     # Plugin-latain
├── ShipHydroSim.Demo/           # Konsoli-demo
└── Example.WavePlugin/          # Esimerkki-plugin
```

## 🚀 Käyttöönotto

### Vaatimukset
- .NET 10.0 SDK
- Windows/Linux/macOS (cross-platform)
- GPU ei pakollinen (CPU-pohjainen, optimoitu)

### Rakentaminen
```powershell
git clone https://github.com/JaakkoJSeppala/SPHDEM-Simulation.git
cd SPHDEM-Simulation
dotnet build ShipHydroSim.sln -c Release
```

### GUI-sovelluksen ajaminen
```powershell
cd ShipHydroSim.App
dotnet run -c Release
```

**Käyttö:**
1. Valitse skenaario: "Dam Break" tai "Wave Scenario"
2. Jos valitset "Wave Scenario", valitse aaltotyyppi (Sine/Stokes/Irregular)
3. Paina **Start** aloittaaksesi simulaation
4. Käytä hiirtä kameran pyörittämiseen viewportissa
5. Seuraa tilastoja: partikkelimäärä, FPS, laivan keinunta (roll/pitch/heave)

## 🧪 Toteutetut ominaisuudet

### SPH (Smooth Particle Hydrodynamics)
- **Kernel-funktiot**: Cubic Spline (M4 B-spline)
- **Tiheys**: ρᵢ = Σⱼ mⱼ W(rᵢ - rⱼ, h)
- **Paine**: Valittavissa Tait EOS (p = k((ρ/ρ₀)^γ - 1)) tai lineaarinen (p = c²(ρ - ρ₀))
- **Voimat**: Paine (symmetrinen SPH), viskositeetti (XSPH), pintajännitys (valinnainen)
- **Naapurihaku**: Spatial Hash (O(1) insertio, O(N_local) kysely)
- **Integraatio**: Explicit Euler, adaptive timestep (CFL + performance)
- **Rajaehdot**: Reflective tank walls (configurable domain boundaries)

### DEM (Discrete Element Method)
- **6DOF Rigid Body**: Position, velocity, orientation (quaternion)
- **Inertia tensor**: Box-approximation laivarungolle
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

## ⚙️ Parametrit & säätö

### Suorituskyky
- `MaxParticles`: 10000 (memory limit)
- `TargetFPS`: 30-45 (adaptive throttling)
- `AdaptiveTimeStep`: true (performance + CFL)
- `UseLinearEOS`: true (faster than Tait)
- `EnableSurfaceTension`: false (CPU savings)
- `NeighborRadiusFactor`: 2.0 (reduce to 1.8 for speed)

### SPH-parametrit
- `SmoothingLength`: 0.16-0.25 (1.2-1.5 × spacing)
- `RestDensity`: 1000 kg/m³
- `Stiffness`: 20000 Pa (if using Tait)
- `SoundSpeed`: 25 m/s (if using linear EOS)
- `Viscosity`: 0.05 Pa·s

### Laivan parametrit
- `GM_Roll`: 0.5 m (metacentric height, roll stability)
- `GM_Pitch`: 0.3 m (pitch stability)
- `DampingCoeff`: 0.1 (hydrodynamic damping)
- `BlockCoefficient`: 0.7 (hull fullness, mass/buoyancy)

### Aallot
- **Sine**: Amplitude 0.3m, λ=4m, T=2s
- **Stokes**: Amplitude 0.4m, λ=4m, T=2s
- **Irregular**: Hs=0.5m, Tp=2.5s, 20 components

## 🔌 Plugin-arkkitehtuuri

Luo oma plugin toteuttamalla `IShipSimPlugin`:

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

Pluginit ladataan dynaamisesti `Plugins/`-kansiosta.

## 📊 Seuraavat vaiheet

### Lähiajan parannukset
- [ ] UI-liukusäätimet parametreille (GM, damping, wave amplitude)
- [ ] Time-series plotting (roll/pitch/heave historia)
- [ ] CSV-vienti simulaatiodatasta
- [ ] Mesh-pohjainen laiva-geometria (STL/OBJ import)

### Fysiikan laajennukset
- [ ] PCISPH (Predictive-Corrective, parempi inkompressibiliteetti)
- [ ] Verlet/Leapfrog integraattori (energia-konservatiivisuus)
- [ ] Turbulenssi-mallit (k-ε, LES)
- [ ] Added mass -termit (lisämassa-efekti)
- [ ] Wave radiation damping

### Suorituskyky
- [ ] GPU-kiihdytys (CUDA/OpenCL/Compute Shaders)
- [ ] Parallel.For SPH-loopeihin
- [ ] Adaptive smoothing length
- [ ] Multi-level spatial hash

### Validointi & Benchmarkit
- [ ] Dam break: vertailu kokeelliseen dataan
- [ ] Sloshing tank: oma-frekvenssi-analyysi
- [ ] Ship roll decay: vaimennus-kerroin-estimaatio
- [ ] Konvergenssi-testit (particle spacing, timestep)

## 📚 Viitteet

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

## 🐛 Tunnetut rajoitukset

- Tällä hetkellä vain laatikkopohjainen laiva-geometria (mesh tulossa)
- Törmäykset vain laivan ja fluidin välillä (ei laiva-laiva)
- CPU-pohjainen (GPU-kiihdytys kehityksessä)
- Spatial hash fixed cell size (adaptive tulossa)
- Ei periodiset rajaehdot (vain reflektoivat seinät)

## 🤝 Kontribuutiot

Tämä on akateeminen tutkimusprojekti. Jos haluat ehdottaa parannuksia:
1. Avaa issue GitHubissa
2. Fork repository ja tee pull request
3. Varmista, että koodi kääntyy (`dotnet build`)
4. Lisää kommentit/dokumentaatio muutoksille

## 📝 Lisenssi

Tämä projekti on kehitetty tutkimustarkoituksiin Helsingin yliopiston gradutyöhön.

## 👤 Tekijä

**Jaakko Seppälä**  
Computational Science, University of Jyväskylä  
Gradu: "3D Ship Hydrodynamics using SPH-DEM Coupling"  
GitHub: [@JaakkoJSeppala](https://github.com/JaakkoJSeppala)

---

**Versio**: 1.0.0 (November 2025)  
**Status**: Active development  
**Target**: M.Sc. Thesis (Computational Science)
