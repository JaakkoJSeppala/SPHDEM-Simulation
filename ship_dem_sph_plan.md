# Implementation Plan: Ship, SPH, DEM, and Coupled Simulation

## 1. Ship Representation
- **Geometry:**
  - Represent the ship as a polygon or parametric shape (rectangle, ellipse, or imported mesh).
  - Store ship position, orientation, and velocity.
- **Visualization:**
  - Draw the ship on the simulation canvas as a filled polygon or image overlay.
  - Update position and orientation each frame.
- **Physics:**
  - Ship interacts with SPH water via buoyancy and drag forces.
  - Ship can be static or dynamic (responding to water and damper forces).

## 2. SPH (Smoothed Particle Hydrodynamics)
- **Simulates:**
  - Water or fluid domain.
  - Particle-based fluid dynamics: pressure, viscosity, gravity, and boundary interactions.
- **Use Cases:**
  - Water waves, splashes, and free-surface flows.
  - Fluid-structure interaction (e.g., ship floating, iceberg melting).
- **Implementation:**
  - Continue with current SPHParticle and SPHSimulator classes.
  - Add boundary conditions for ship hull and domain edges.
  - Compute forces on ship from SPH particles (buoyancy, pressure).

## 3. DEM (Discrete Element Method)
- **Simulates:**
  - Granular materials (e.g., damper particles, icebergs, debris).
  - Particle-particle and particle-boundary collisions, friction, and damping.
- **Use Cases:**
  - Ship dampers (granular beds), icebergs, floating debris.
  - DEM particles can be rigid or deformable.
- **Implementation:**
  - DEMParticle and DEMSimulator classes for particle motion and collision.
  - Visualize DEM particles as colored ellipses or polygons.
  - Add interaction with ship and boundaries.

## 4. Coupled SPH-DEM Simulation
- **Simulates:**
  - Interaction between fluid (SPH) and granular/solid (DEM) domains.
  - Examples: Ship with granular dampers, icebergs in water, debris floating/moving in waves.
- **Coupling Mechanism:**
  - Exchange forces: SPH applies drag/buoyancy to DEM, DEM applies resistance/obstruction to SPH.
  - Use SPHDEMCoupler class to manage force exchange and update both domains.
- **Implementation Steps:**
  1. Update SPHSimulator to compute forces on DEM particles (fluid drag, buoyancy).
  2. Update DEMSimulator to apply reaction forces to SPH particles (momentum exchange).
  3. Synchronize time-stepping and update both domains each frame.
  4. Visualize both particle types and ship in the UI.

## 5. Example Scenarios
- **Ship with Granular Dampers:**
  - Ship floats in SPH water, dampers modeled as DEM particles inside hull.
  - Coupling: Water motion affects damper particles, dampers absorb ship motion.
- **Icebergs in Water:**
  - Icebergs as DEM clusters, water as SPH.
  - Coupling: Water applies buoyancy and drag to icebergs, icebergs displace water.
- **Floating Debris:**
  - DEM particles represent debris, SPH simulates water.
  - Coupling: Debris floats, moves, and interacts with waves.

## 6. Documentation and Academic Notes
- Clearly separate SPH (fluid) and DEM (granular/solid) domains in code and visualization.
- Document all physical assumptions, coupling algorithms, and limitations.
- Reference academic literature for SPH-DEM coupling (e.g., fluid-granular interaction, buoyancy models).
- Ensure reproducibility and extensibility for future research.

---
*This plan provides a modular, extensible approach for simulating ships, water, and granular materials using SPH, DEM, and their coupling. Each domain is clearly defined, and coupling mechanisms are documented for academic transparency.*
