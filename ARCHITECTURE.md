# Projektin arkkitehtuuri-dokumentaatio

## Yleiskuva

ShipHydroSim on modulaarinen simulaatio-ohjelmisto, joka yhdistää SPH- ja DEM-menetelmät laivan hydrodynamiikan tutkimukseen.

## Moduulit

### 1. ShipHydroSim.Core (net10.0)

**Geometry/** - Matematiikka ja geometria
- `Vector3.cs`: 3D-vektorit (positiot, nopeudet, voimat)
- `Quaternion.cs`: Rotaatiot (DEM rigid bodies)
- `Matrix3x3.cs`: Inertiamomentti-tensorit

**SPH/** - Smooth Particle Hydrodynamics
- `Particle.cs`: SPH-partikkeli (massa, tiheys, paine, nopeus)
- `KernelFunctions.cs`: Kernel-funktiot smoothing:iin
  - Cubic Spline (M4 B-spline)
  - Wendland C2 (kompakti tuki)
- `SPHSolver.cs`: WCSPH-solveri
  - Tiheyden laskenta
  - Paine (Tait equation)
  - Viskositeetti (XSPH)
  - Explicit Euler -integraatio

**DEM/** - Discrete Element Method
- `RigidBody.cs`: Jäykkä kappale (laiva, debris)
  - Lineaarinen liike (position, velocity, force)
  - Kulmamomentum (orientation, angular velocity, torque)
  - Inertiamomentti-tensori
- `IShape`: Geometria-rajapinta
- `SphereShape`: Pallogeometria (testaus)

**Spatial/** - Spatiaalinen datarakenne
- `SpatialHash.cs`: Grid-pohjainen naapurihaku
  - O(1) insertion
  - O(N_local) query

**Interfaces**
- `ISimulationSolver.cs`: Solver-rajapinta
- `SimulationParameters.cs`: Parametrit (SPH, DEM, domain)

### 2. ShipHydroSim.PluginAPI (net10.0)

Plugin-rajapinnat laajennettavuudelle:
- `IShipSimHost`: Host-sovellus (pääsy solveriin, parametreihin)
- `IShipSimPlugin`: Plugin-perusrajapinta
- `IForceModelPlugin`: Custom voimamallit
- `IIntegratorPlugin`: Custom integraattorit

### 3. ShipHydroSim.PluginHost (net10.0)

- `PluginLoader.cs`: Dynaamisesti lataa .dll-plugineja
  - Reflection + AssemblyLoadContext
  - Auto-discovery `Plugins/`-kansiosta

### 4. ShipHydroSim.App (net9.0)

Avalonia-pohjainen GUI (cross-platform):
- MVVM-arkkitehtuuri
- 3D-visualisointi (tulossa: Helix Toolkit / Silk.NET)
- Real-time simulaatio

### 5. ShipHydroSim.Demo (net10.0)

Konsoliohjelma testaukseen:
- Dam break -skenaario
- ~6500 partikkelia
- Outputtaa statistiikkaa (keskikorkeus, nopeus, tiheys)

### 6. Example.WavePlugin (net10.0)

Esimerkki custom-pluginista (placeholder tällä hetkellä)

## Datavirta

```
User Input → ShipHydroSim.App (GUI)
                ↓
         IShipSimHost
                ↓
         SPHSolver.Step()
                ↓
    ┌───────────┴────────────┐
    ↓                        ↓
SpatialHash          Particles (6500+)
(naapurihaku)        (Position, Velocity, Density, Pressure)
    ↓                        ↓
FindNeighbors()    ComputeDensityAndPressure()
    └────────────┬───────────┘
                 ↓
         ComputeForces()
         (Pressure, Viscosity, Gravity)
                 ↓
         Integrate()
         (Euler: v += a·dt, x += v·dt)
                 ↓
         ApplyBoundaryConditions()
                 ↓
         Visualization / Output
```

## Suorituskyky

### Nykyinen (Debug-build)
- 6468 partikkelia
- Timestep: 0.001 s
- Smoothing length: 0.2 m
- Noin 500 askelta tulostetaan terminaaliin

### Optimointipotentiaali
- **Spatial hashing**: Cell size = 2h (optimaalinen)
- **Rinnakkaistaminen**: 
  - `Parallel.For` density/force -laskennoissa
  - GPU (CUDA/OpenCL) suurille partikkelimäärille (>100k)
- **Integraattori**: Verlet → parempi energia-konservatiivisuus
- **Kernel**: Wendland C2 → nopeampi kuin Cubic Spline

## Laajennettavuus

### Uudet fysiikkamallit
Lisää plugin:
```csharp
public class TurbulencePlugin : IForceModelPlugin
{
    public void ComputeForces()
    {
        // Lisää turbulenssi-voimat partikkeleille
    }
}
```

### Uudet geometriat
Toteuta `IShape`:
```csharp
public class MeshShape : IShape
{
    public List<Triangle> Triangles { get; set; }
    public double GetBoundingRadius() { /* ... */ }
}
```

### Visualisointi
Lisää Avalonia-näkymään:
- Helix Toolkit 3D (WPF-style)
- Silk.NET (OpenGL/Vulkan)
- SkiaSharp (2D-leikkaukset)

## Tulevat ominaisuudet

### Prioriteetti 1 (Graduun)
1. **DEM collision detection**
   - Sphere-sphere
   - Sphere-plane
   - Hertz contact model

2. **SPH ↔ DEM coupling**
   - Boundary forces: nesteen paine → laivan pinta
   - Two-way: laivan liike → nesteen siirtyminen

3. **Visualisointi**
   - 3D partikkelit (pisteet/pallot)
   - Laiva-mesh
   - Vektorikentät (nopeus, paine)

### Prioriteetti 2 (Tutkimus)
4. **PCISPH** (Predictive-Corrective)
   - Parempi kuin WCSPH: vähemmän kompressibiliteettia

5. **Adaptive time stepping**
   - CFL-ehto: dt ≤ 0.25 h / max(|v|)

6. **Surface tension**
   - Kapillaari-ilmiöt pienillä mittaluokilla

7. **Mesh-geometria**
   - STL/OBJ import
   - Raycast/SDF boundary handling

## Testaus

### Validointi-skenaariot
1. **Dam break** (toteutettu)
   - Vesipatsaan romahdus
   - Vertaa kokeelliseen dataan (Martin & Moyce 1952)

2. **Hydrostatic** (tulossa)
   - Paine kasvaa syvyyden mukaan: p = ρgh

3. **Poiseuille flow** (tulossa)
   - Laminar virtaus putkessa
   - Analyttinen ratkaisu olemassa

4. **Floating body** (tulossa)
   - Archimedes-voima
   - Stabiilisuus (GM-korkeus)

## Viitteet koodiin

### SPH-teoria
- `SPHSolver.ComputeDensityAndPressure()`: Monaghan (1992), Eq. 2-4
- `SPHSolver.ComputeForces()`: Pressure term (Monaghan 1992, Eq. 10)
- `KernelFunctions.CubicSpline()`: Liu & Liu (2003), Eq. 3.32

### DEM-teoria
- `RigidBody`: Euler equations (gyroscopic effect tulee myöhemmin)
- Contact models: Cundall & Strack (1979)

## Gradun rakenne (ehdotus)

1. **Johdanto**
   - Laivan hydrodynamiikka
   - SPH + DEM motivaatio

2. **Teoria**
   - SPH-johto Navier-Stokesista
   - DEM rigid body dynamics
   - Coupling: boundary conditions

3. **Toteutus**
   - Arkkitehtuuri (tämä dokumentti)
   - Algoritmit (spatial hashing, kernel-funktiot)
   - Plugin-järjestelmä

4. **Validointi**
   - Dam break, sloshing
   - Vertaa referensseihin

5. **Tulokset**
   - Laiva sloshing-testissä
   - Suorituskyky-analyysi

6. **Johtopäätökset**
   - Milloin SPH+DEM on sopiva?
   - Rajoitukset, tulevaisuus

---

**Yhteenveto**: Projekti on hyvässä pohjassa gradutyöhön. SPH-solver toimii, modulaarinen rakenne tukee laajennuksia, ja seuraavat askeleet (DEM collision, coupling, visualisointi) on selkeästi määritelty.
