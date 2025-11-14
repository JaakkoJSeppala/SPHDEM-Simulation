# Quick Start Guide

## 5 minuutin pika-aloitus

### 1. Rakenna projekti
```powershell
cd C:\Users\jaakk\Desktop\Gradu\ShipHydroSim
dotnet build -c Release
```

### 2. Aja SPH-demo
```powershell
cd ShipHydroSim.Demo
dotnet run -c Release
```

**Mitä tapahtuu**: 6500 vesipartikkelia simuloi "dam break" -skenaarion 3 sekunnin ajan.

### 3. Tutki tulokset
Terminaalissa näet:
```
Time(s) Particles   AvgHeight   AvgVel
0.500   6468        2.345       1.234
1.000   6468        1.876       0.987
...
```

## Muokkaa parametreja

### SPHSolver-parametrit (`ShipHydroSim.Demo/Program.cs`)

```csharp
var solver = new SPHSolver
{
    SmoothingLength = 0.2,      // h: partikkelin vaikutussäde (m)
    RestDensity = 1000.0,        // ρ₀: veden tiheys (kg/m³)
    Stiffness = 20000.0,         // k: paineen jäykkyys (Pa)
    Viscosity = 0.05,            // μ: viskositeetti (Pa·s)
    Gravity = new Vector3(0, -9.81, 0),  // g: gravitaatio (m/s²)
    TimeStep = 0.001             // dt: aikaväli (s)
};
```

**Kokeile**:
- Suurempi `Stiffness` → vähemmän kompressibiliteettia
- Suurempi `Viscosity` → hitaampi virtaus
- Pienempi `TimeStep` → tarkempi, mutta hitaampi

### Partikkelien määrä

Muuta `Program.cs`:ssä:
```csharp
// Enemmän partikkeleja → tarkempi, mutta hitaampi
double spacing = 0.1;  // Pienempi spacing = enemmän partikkeleita

// Vähemmän partikkeleja → nopeampi, mutta karkeampi
double spacing = 0.2;
```

## Luo oma skenaario

### Esimerkki: Putoava vesipisara

```csharp
// Luo pallo-muotoinen vesipisara (r = 0.5 m)
int particleId = 0;
double radius = 0.5;
double spacing = 0.1;
Vector3 center = new(5, 8, 5); // Keskellä, ylhäällä

for (double x = -radius; x <= radius; x += spacing)
{
    for (double y = -radius; y <= radius; y += spacing)
    {
        for (double z = -radius; z <= radius; z += spacing)
        {
            double dist = Math.Sqrt(x*x + y*y + z*z);
            if (dist <= radius)
            {
                var pos = center + new Vector3(x, y, z);
                solver.AddParticle(new Particle(particleId++, pos, 0.02));
            }
        }
    }
}
```

### Esimerkki: Sloshing (aaltoilu säiliössä)

```csharp
// Täytä laatikko vedellä
for (double x = 0.5; x < 9.5; x += spacing)
{
    for (double y = 0.1; y < 4.0; y += spacing)
    {
        for (double z = 0.5; z < 9.5; z += spacing)
        {
            var particle = new Particle(particleId++, new Vector3(x, y, z), 0.02);
            
            // Anna alkuperäinen nopeus (sloshing-aalto)
            particle.Velocity = new Vector3(
                2.0 * Math.Sin(x * Math.PI / 10.0), // Vaakasuora aalto
                0,
                0
            );
            
            solver.AddParticle(particle);
        }
    }
}
```

## Tallenna tulokset tiedostoon

Lisää `Program.cs`:ään:

```csharp
using System.IO;

// Simulaation jälkeen
using var writer = new StreamWriter("results.csv");
writer.WriteLine("Time,ParticleID,X,Y,Z,VelX,VelY,VelZ,Density,Pressure");

foreach (var p in solver.Particles)
{
    writer.WriteLine($"{simulationTime},{p.Id},{p.Position.X},{p.Position.Y},{p.Position.Z}," +
                     $"{p.Velocity.X},{p.Velocity.Y},{p.Velocity.Z},{p.Density},{p.Pressure}");
}
```

Avaa `results.csv` Excelissä tai Pythonilla (Matplotlib/ParaView).

## Visualisointi Pythonilla

```python
import pandas as pd
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

# Lue data
df = pd.read_csv('results.csv')

# 3D scatter plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.scatter(df['X'], df['Y'], df['Z'], c=df['Density'], cmap='viridis', s=5)
ax.set_xlabel('X (m)')
ax.set_ylabel('Y (m)')
ax.set_zlabel('Z (m)')
plt.colorbar(ax.scatter(df['X'], df['Y'], df['Z'], c=df['Density'], cmap='viridis'))
plt.title('SPH Particle Distribution')
plt.show()
```

## Seuraavat askeleet

1. **Lisää DEM rigid body**:
   ```csharp
   var ship = new RigidBody(0, new Vector3(5, 3, 5), 100.0, new SphereShape(1.0));
   // Törmäystarkistus tulossa...
   ```

2. **Luo oma plugin** (ks. `ARCHITECTURE.md`)

3. **Käynnistä GUI** (kun visualisointi toteutettu):
   ```powershell
   cd ShipHydroSim.App
   dotnet run
   ```

## Debuggaus

### Simulaatio räjähtää (partikkelit lentävät ulos)
- **Syy**: Liian suuri `TimeStep`
- **Ratkaisu**: Pienennä `TimeStep` (esim. 0.0005)

### Partikkelit kompressoituvat liikaa
- **Syy**: Liian pieni `Stiffness`
- **Ratkaisu**: Suurenna `Stiffness` (esim. 50000)

### Simulaatio liian hidas
- **Syy**: Liikaa partikkeleita tai liian pieni `TimeStep`
- **Ratkaisu**: 
  - Suurenna `spacing` (vähemmän partikkeleita)
  - Suurenna `TimeStep` (huolehdi stabiilisuudesta)
  - Käännä Release-modessa: `dotnet run -c Release`

## Lisätietoa

- `README.md`: Yleiskatsaus ja viitteet
- `ARCHITECTURE.md`: Tekninen dokumentaatio
- `ShipHydroSim.Core/SPH/`: Lähdekoodit kommentoitu

**Questions?** Tutki lähdekoodeja tai kysy ohjaajalta!
