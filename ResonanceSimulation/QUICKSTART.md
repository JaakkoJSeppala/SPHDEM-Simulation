# Pika-aloitusohje: Resonanssisimulaatio

## 1. Vaatimukset
- **.NET 8 SDK** ([lataa](https://dotnet.microsoft.com/download))
- **Python 3.10+** (post-processing varten)
- Python-paketit: `pip install pandas matplotlib numpy`

## 2. Kompiloi projekti
```powershell
cd c:\Users\jaakk\Desktop\Gradu\ResonanceSimulation
dotnet build -c Release
```

## 3. Aja simulaatio

### Yksittäinen simulaatio (nopea testi)
```powershell
dotnet run --project ResonanceSimulation.App -c Release -- --frequency 0.6 --damper true --time 10
```

### Täydellinen taajuuspyyhkäisy (graduun)
```powershell
# Ilman damperia + damperilla, 10 taajuutta
dotnet run --project ResonanceSimulation.App -c Release -- --sweep --time 30 --output results
```

**Arvioitu ajoaika:** ~2–4 tuntia (i9-13900K)

## 4. Analysoi tulokset
```powershell
python plot_results.py
```

Tämä luo kuviot:
- `results/summary/fig_resonance_curve.pdf` → Resonanssikäyrä
- `results/summary/fig_pressure_time_f0.60Hz.pdf` → Paine-etenemä
- `results/summary/fig_energy_decay_f0.60Hz.pdf` → Energian vaimeneminen
- `results/summary/fig_damping_ratios.pdf` → Vaimennussuhteet

## 5. Tulokset graduun

### LaTeX-koodi kuville
```latex
\begin{figure}[htbp]
\centering
\includegraphics[width=0.9\textwidth]{ResonanceSimulation/results/summary/fig_resonance_curve.pdf}
\caption{Resonanssikäyrä: maksimipaine seinällä taajuuden funktiona. 
Granulaarivaimennin vähentää resonanssipiikkiä 60--80\,\%.}
\label{fig:resonance_curve}
\end{figure}
```

### Odotetut tulokset (gradu s. 42–50)
- **Resonanssipiikki**: vähenee 60–80% damperilla
- **Vaimennussuhde**: ζ ≈ 0.01–0.02 → 0.09–0.14
- **Resonanssitaajuus**: f₀ ≈ 0.6–0.8 Hz (skaalattu 1:50)

## 6. Vianmääritys

### "FileNotFoundException: CommandLineParser"
```powershell
cd ResonanceSimulation.App
dotnet restore
```

### "Simulaatio kaatuu heti"
- Tarkista muisti: simulaatio tarvitsee ~4–8 GB
- Pienennä `TotalTime` tai kasvata `TimeStep` (SimulationConfig.cs)

### "Python ei löydä tiedostoja"
```powershell
# Tarkista että sweep on ajettu:
ls results/sweep_nodamper/
ls results/sweep_withdamper/
```

## 7. Parametrien muokkaus

Avaa `ResonanceSimulation.Core/SimulationConfig.cs`:

```csharp
// Nopea testi (5 min)
public double TotalTime { get; set; } = 10.0;
public double TimeStep { get; set; } = 0.0002;

// Täysi tarkkuus (20 min)
public double TotalTime { get; set; } = 30.0;
public double TimeStep { get; set; } = 0.0001;

// Suurempi vaimennin
public double DamperMassRatio { get; set; } = 0.20; // 20%

// Isommat pallot
public double ParticleDiameter { get; set; } = 0.008; // 8 mm
```

Compiloi uudelleen: `dotnet build -c Release`

## Ongelmat?
Avaa issue tai kysy!
