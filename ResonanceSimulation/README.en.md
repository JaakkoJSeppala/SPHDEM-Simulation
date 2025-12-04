# Resonance Response Simulation: 1:50 Ballast Tank + Granular Damper

## 🎯 Purpose
This C# simulation answers the thesis question:
> "How effectively does SPH–DEM reduce sloshing-induced structural loads at resonance?"

## 📐 Geometry (1:50 scaled Aframax)
- **Full-scale tank**: 15 m × 20 m
- **Model**: 0.30 m × 0.40 m
- **Fill ratio**: 50% (0.20 m)
- **Damper**: Bottom compartment, d=4–6 mm spheres, 10–15% of fluid mass

## 🌊 Motion
Sinusoidal horizontal displacement: `x(t) = A·sin(2πft)`
- **Amplitude**: A = 0.01–0.03 m
- **Frequency**: f = 0.2–1.0 Hz (includes resonance ~0.6–0.8 Hz)

## 📊 Measurements
1. **Resonance curve**: max(p) vs. f → with and without damper
2. **Pressure time history**: p(t) at resonance frequency
3. **Free surface**: h(t) as a function of time
4. **Energy**: Ek(t) → damping ratio ζ

## ⚙️ Simulation Parameters

- **Amplitude (A):** 0.02 m (default)
- **Excitation frequency (f):** 0.6 Hz (default, resonance)
    - This is the frequency of the tank's horizontal motion:
      $x(t) = A \cdot \sin(2\pi f t)$
    - The default (0.6 Hz) is the resonance frequency of the tank (maximum sloshing response).
    - You can sweep f from 0.2 to 1.0 Hz to generate a resonance curve.
- **Damper:** enabled/disabled
- **Simulation time:** 10–30 s

You can adjust these in the web UI or via command line:

```sh
dotnet run --project ResonanceSimulation.App -- --frequency 0.6 --damper true --time 10
```

## 🚀 How to Run the Web Application

1. Build the project:
   ```bash
   dotnet build -c Release
   ```

2. Start the web server:
   ```bash
   dotnet run --project ResonanceSimulation.Web/ResonanceSimulation.Web.csproj -c Release
   ```

3. Open your browser and go to:
   ```
   http://localhost:5000
   ```

4. Use the web interface to set simulation parameters and start the simulation. Visualization and results update automatically in real time.

No command line usage is required for running simulations—everything is controlled via the browser.

## 📁 Results Structure
```
results/
├── sweep_nodamper/
│   ├── f_0.20Hz.csv
│   ├── f_0.30Hz.csv
│   └── ...
├── sweep_withdamper/
│   ├── f_0.20Hz.csv
│   └── ...
└── summary/
    ├── resonance_curve.csv
    ├── damping_ratios.csv
    └── energy_decay.csv
```

## 🔬 Expected Results (based on thesis)
- Resonance peak is reduced by **60–80%** with damper
- Damping ratio: ζ ≈ 0.01–0.02 → 0.09–0.14
- Maximum pressure: p_max decreases by **50–70%**

## 📖 References to Thesis
- Method: thesis pp. 21–41 (SPH–DEM theory)
- Parameters: thesis pp. 42–45 (optimization d=4–6 mm)
- Validation: thesis pp. 46–50 (benchmarks)
