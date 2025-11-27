# Ship and Water Interaction Documentation

## Ship Visualization and Physics
- The ship is represented as a rectangle on the simulation canvas, with configurable position, size, and orientation.
- Boundaries are drawn as black lines to define the simulation domain.

## Physical Interaction with Water (SPH)
- The ship interacts with water particles using physically-based forces:
  - **Buoyancy:** Calculated by integrating the pressure exerted by SPH particles beneath the ship hull. The net upward force equals the weight of displaced water (Archimedes' principle).
  - **Drag:** The ship experiences resistance proportional to its velocity and the relative velocity of nearby SPH particles. Drag force is computed using standard fluid dynamics equations.
  - **Pressure Distribution:** The pressure field from SPH particles is sampled at the ship hull to compute local forces and moments, allowing for realistic floating and tilting behavior.

## Implementation Plan
1. **Ship State:**
   - Store ship position, velocity, orientation, and mass.
   - Update ship state each frame using Newton's laws: $F = ma$, $\tau = I\alpha$.
2. **Force Calculation:**
   - For each SPH particle near the ship hull, compute pressure and velocity relative to the ship.
   - Integrate pressure over the submerged area for buoyancy.
   - Sum drag forces from local fluid velocity.
3. **Motion Update:**
   - Apply net force and torque to update ship velocity and position.
   - Enforce boundary conditions to keep the ship within the domain.
4. **Visualization:**
   - Update ship drawing based on new position and orientation.
   - Optionally visualize force vectors and pressure distribution for analysis.

## Physics References
- Buoyancy: $F_{buoy} = \rho_{water} V_{sub} g$
- Drag: $F_{drag} = \frac{1}{2} C_d \rho_{water} A v^2$
- Pressure: $p = \rho_{water} g h$
- Equations of motion: $m \frac{d^2x}{dt^2} = F_{net}$, $I \frac{d^2\theta}{dt^2} = \tau_{net}$

## Academic Notes
- All physical interactions are implemented using first principles and referenced equations.
- The simulation is modular, allowing for extension to more complex hull shapes and multi-body interactions.
- Code comments and documentation ensure reproducibility and clarity for academic use.

---
*This document describes the visualization, physical modeling, and implementation of ship-water interaction using SPH. All forces and updates follow established fluid dynamics and mechanics principles for academic rigor.*
