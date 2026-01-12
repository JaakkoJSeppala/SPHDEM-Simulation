# WP1: Geometry and Particle Initialization – Documentation

This document describes the rationale, methodology, and parameter choices for the initial geometry and particle setup used in the dam-break + cube SPH benchmark simulation. The goal is to ensure that all modeling decisions are transparent, justified, and reproducible.

## Objective
To create a simple, well-defined initial state for validating the SPH code's ability to represent free-surface flow and rigid body interaction, using a literature-based benchmark geometry. This setup is the foundation for all subsequent validation and extension steps.

## Domain and Geometry
- **Tank dimensions:** Width $L = 1.0$ m, Height $H = 0.5$ m
  - *Rationale:* A rectangular tank is the standard geometry for dam-break and sloshing benchmarks, allowing direct comparison to published results and analytical solutions.
- **Fluid region:** Initial fill height $h = 0.3H = 0.15$ m
  - *Rationale:* Partial fill ensures a free surface and allows observation of wave propagation and hydrostatic equilibrium.
- **Rigid body (cube):** Side length $a = 0.08$ m, placed at $(x, y) = (0.7, 0.1)$
  - *Rationale:* The cube is positioned away from the initial fluid region to allow for clear observation of fluid–solid interaction after the dam break. The size is chosen to be large enough for meaningful interaction but small enough to avoid dominating the flow.

## Particle Discretization
- **Particle spacing:** $\Delta x = 0.02$ m
  - *Rationale:* Chosen to balance computational cost and resolution. This spacing is fine enough to resolve the cube and fluid interface, but coarse enough for rapid prototyping and visualization.
- **Fluid particles:** Placed on a regular grid in the filled region
  - *Rationale:* Uniform grid ensures consistent density and avoids initialization artifacts. Each particle is assigned a position, mass, and initial velocity (zero).
- **Cube particles:** Placed on a regular grid within the cube's volume
  - *Rationale:* The cube is represented as a rigid collection of particles, which allows for later extension to rigid body dynamics. The grid matches the fluid particle spacing for consistent interaction.
- **Wall particles:** Placed along all four tank boundaries (bottom, left, right, top)
  - *Rationale:* Wall particles prevent fluid escape and enforce boundary conditions. Their spacing matches the fluid and cube for stability and accuracy.

## Visualization and Verification
- **Scatter plot of all particles:**
  - Blue: Fluid
  - Red: Cube
  - Black: Walls
- *Rationale:* Immediate visual feedback ensures that the geometry, particle placement, and boundaries are correct before proceeding to dynamics. This step is essential for debugging and for documenting the initial state in the thesis.

## Assumptions and Simplifications
- 2D geometry (no $z$-direction)
- No initial overlap between fluid and cube
- All particles have the same mass and spacing
- No adaptive refinement or variable particle size at this stage

## Reproducibility
- All parameters (domain size, particle spacing, cube size and position) are explicitly defined in the script.
- The script can be run as-is to regenerate the initial state and figure.
- This setup matches common benchmarks in the SPH literature, enabling direct comparison.

## Next Steps
- Use this initial state as input for hydrostatic equilibrium tests (WP2)
- Extend the cube to a true rigid body for force and motion validation (WP3–WP4)

---

*Prepared for thesis validation workflow. All choices are justified for clarity, reproducibility, and direct comparison to published benchmarks.*
