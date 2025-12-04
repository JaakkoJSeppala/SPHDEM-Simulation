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

## 📚 Theories and Models Simulated

This simulator models the following physical and mathematical theories:

- **Smoothed Particle Hydrodynamics (SPH):**
  - Mesh-free method for simulating fluid flow, especially free-surface and sloshing phenomena.
  - Uses a weakly compressible SPH (WCSPH) formulation for liquid dynamics.

- **Discrete Element Method (DEM):**
  - Models granular materials (damper particles) and their interactions.
  - Handles particle-particle and particle-wall collisions, restitution, friction, and dissipation.

- **Coupled SPH–DEM System:**
  - Two-way coupling: fluid (SPH) and particles (DEM) interact, exchanging forces and momentum.
  - Essential for simulating the effect of granular dampers on sloshing.

- **Sloshing Dynamics in Ballast Tanks:**
  - Tank excitation (e.g., pitch motion) induces liquid sloshing and resonance.
  - Simulation focuses on resonance, peak loads, and damping effects.

- **Energy Dissipation Mechanisms:**
  - Collision-dominated dissipation (particle impacts).
  - Viscous and drag contributions (fluid-particle and fluid-wall interactions).

- **Sensitivity Analysis:**
  - Studies the effect of restitution, mass ratio, friction, particle size, and fill ratio on damping performance.

- **Validation and Benchmarking:**
  - Analytical tests and classical dam-break benchmarks are used to verify the model.

---

This documentation helps clarify the scientific basis and scope of the simulation, supporting further development and validation.
