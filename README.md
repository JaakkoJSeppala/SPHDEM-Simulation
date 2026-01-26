
# SPH-DEM Simulation Project

This project provides a modular Python implementation for simulating coupled Smoothed Particle Hydrodynamics (SPH) and Discrete Element Method (DEM) systems. It is designed to answer the following research questions:

1. How does granular material affect free-surface motion and impact pressure in a tank?
2. Which parameters (particle size, fill ratio, damping coefficient) most strongly influence energy dissipation efficiency?
3. How well does the coupled SPH-DEM approach reproduce benchmark results from prior studies?

## Structure
- `sph/` — SPH fluid simulation module
- `dem/` — DEM particle simulation module
- `sim/` — Coupled SPH-DEM simulation runner
- `analysis/` — Parameter sweep automation and result analysis
- `visualization/` — Plotting and reporting tools

## Getting Started
1. Install Python 3.10+ and recommended packages (see requirements.txt)
2. Run example simulations from the `sim/` directory
3. Analyze results using scripts in `analysis/` and `visualization/`

## License
MIT
