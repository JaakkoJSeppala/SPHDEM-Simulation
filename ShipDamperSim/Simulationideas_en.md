Below is a working way to build a thesis (and at the same time a simulator) on the topic of **SPH + DEM + granular dampers** and their effect on **ship hydrodynamics** — specifically so that you get:

- **C#-based implementation**
- **3D visualization** (water + ship + granulate)
- **adjustable granular damper parameters** (masses, grain size, fill ratio, container geometry, placement in the ship, frictions, etc.)
- **thorough documentation** (especially damper installation in the ship and modeling assumptions)

This is a “blueprint” that can be implemented step by step and has a realistic chance of being completed as a thesis.

---

## 1) Narrow the thesis to a clear question

Granular dampers mainly affect the ship's **motion responses** (roll/pitch/heave) and damping. A good main research question for the thesis:

**“How do the location, fill ratio, and grain parameters of a granular damper change the ship's roll (RAO / damping ratio / transient response) under wave excitation?”**

Measurements:

- roll angle ( \phi(t) ), roll angle peaks, settling time, damping ratio
- frequency response / RAO (if you do sine waves / sweep)
- energy dissipation (DEM contact work + friction work)

Keep the main result: **the damper optimizes rolling** in a certain parameter and placement range.

---

## 2) Modeling strategy: “the minimum that works” but is credible

Full 3D SPH + 6DOF + DEM is heavy. In a thesis, it is advisable to make a _controlled_ whole:

### Phase A (the “solid base” of the thesis)

- **Ship motion 1–2 DOF** (e.g., only roll, or roll + heave)
- **SPH** only for evaluating the free surface and pressure field around the ship _or_ even easier: SPH as a “research prototype” and hydrodynamics partly parameterized.

### Phase B (extension, if time allows)

- **6DOF rigid body** and SPH pressure integration on the hull
- Better wave and boundary condition models

**DEM for the granular damper** is the core of the thesis. SPH can be either:

1. “real hydrodynamics” (pressure sum from SPH particles to the hull), or
2. “free surface visualization + simplified hydrodynamics” (e.g., linear roll-restoring + hydrodynamic extra damping, and the damper brings additional energy loss).

If you want a reliably working whole in C#, I recommend:

- **Damper (DEM) fully physical**
- **Ship hydrodynamics initially simplified**, but validatable (e.g., roll-decay test)
- SPH included **for visualization and load estimation** later

This way, the thesis remains manageable and the results are still significant.

---

## 3) Simulator architecture in C# (clear and extensible)

### Modular structure

1. **Physics.Core**
   - vector math, integrators, timestep, energy tracking
2. **RigidBody**
   - ship as a rigid body (mass, inertia, 1–6 DOF)
3. **SPH**
   - fluid particles, neighbor search, density/pressure/viscosity, boundaries
4. **DEM**
   - grain particles, contact model, wall contacts (damper container inner surfaces), friction, restitution
5. **Coupling**
   - ship motion ↔ damper: container moves with the ship; grain forces sum to a moment on the ship
   - (later) SPH ↔ ship: pressure integration on the hull
6. **Rendering**
   - 3D, instanced rendering for particles, debug visualization (force vectors, pressure colors, contacts)
7. **UI / ExperimentRunner**
   - parameter sweeps, saving, analysis (CSV, JSON), repeatable runs

### Important principle

**Physics loop must be deterministic**: same seed → same result (good for a thesis).

---

## 4) Technology choices C# + 3D

**Recommendation (most practical):**

- **Unity + C#**
  - Rendering and UI (sliders, settings) are easiest
  - GPU instancing for particles is possible
  - Simulation can be run in “FixedUpdate” or its own thread (carefully)

**Alternative (cleaner “engineering tool”):**

- **Stride (formerly Xenko) + C#**
- **MonoGame** if you want to do everything yourself (more work)

For the thesis, Unity gives the fastest impressive 3D and control panels.

---

## 5) For DEM granular damper: what you need to model

### (a) Damper geometry and installation in the ship (must be documented!)

Model the damper as a “container” (rigid container), which is attached to the ship's hull at a certain:

- **location** in the ship's coordinate system (x, y, z)
- **orientation** (is the container tilted)
- **attachment** (default: fully rigid attachment to the hull)
- **container dimensions** (length, width, height)
- **inner surface material parameters** (friction coefficient, restitution coefficient)

In documentation, draw:

- ship coordinate system
- damper location dimensions (distance from center of gravity, height position)
- attachment assumption (welded / bolted → in simulation “rigidly attached”)

### (b) Grain model

Grains: spheres (easy for DEM). Parameters:

- grain radius (or distribution)
- density (→ mass)
- fill ratio (volume fraction)
- contact model: **Hertz-Mindlin** or simple spring-damper + Coulomb friction
- wall contacts (grains vs container)

### (c) Coupling to the ship

When the ship rolls, the damper container rolls with it. Grains are affected by:

- gravity in the ship's coordinate system
- “inertial forces” if you simulate in a non-inertial frame, or alternatively:
  - simulate everything in the inertial frame and let the container move as a rigid body with the ship

Easiest:

- ship = rigid body (at least roll)
- container = transform attached to the ship
- grains are integrated in world coordinates, and wall contacts are calculated to the moving surfaces of the container

Moment affecting the ship:

- sum of all grain–wall contact forces (and their moment arms) through the container's attachment point to the ship.

---

## 6) For SPH water: realistic but manageable

In SPH, the most important thing for the thesis's credibility is:

- neighbor search (grid/hashed)
- density and pressure (e.g., Tait equation)
- viscosity (artificial + physical)
- interface (free surface) – at least “water stays together”
- boundaries: tank / sea area

If full hull–water contact is too heavy, make two levels:

1. **SPH tank + simple ship** (e.g., rectangle/wedge) → show the effect visually
2. **Hydrodynamics as a roll equation** and the damper brings extra damping → make the final measurement curves from this

Roll equation in basic form:
[
I \ddot{\phi} + c_h \dot{\phi} + k \phi = M_{wave}(t) + M_{damper}(t)
]

- (M\_{damper}(t)) comes from DEM
- (M\_{wave}(t)) can be sinusoidal or impulse
- (c_h) and (k) can be set from literature / calibrated with roll-decay test

This gives a clear scientific setup: **additional damping from the damper** and its dependence on parameters.

---

## 7) Experiments and validation (the “scientific backbone” of the thesis)

### Minimum experiments that look good in a thesis

1. **Roll decay** without waves
   - give an initial push (e.g., 10°) and observe damping
   - compare: without damper vs with damper
   - result: damping ratio and settling time
2. **Harmonic excitation** (sine wave as moment)
   - different frequencies → RAO curve
   - show how the damper reduces the peak at resonance
3. **Parameter sweep**
   - fill ratio (e.g., 10–90%)
   - grain size
   - friction coefficient (grains–wall)
   - damper location (height and distance from center of gravity)

### Validation

- The DEM part can be validated with a “box-sloshing” type test: known damping/energy loss trends vs fill ratio.
- Roll equation parameters can be calibrated so that “without damper” matches a typical roll-decay curve.

Important in the thesis: you do not claim a “completely real ship”, but that:

- the model is physically justified
- the limitations are clear
- trends are repeatable and parameter-dependent

---

## 8) UI: damper adjustment and running experiments

Make an “Experiment Panel” with:

- damper on/off
- damper location (x,y,z)
- container dimensions
- grain parameters: radius, number/fill ratio, density
- materials: friction, restitution
- ship parameters: inertia, restoring stiffness, hydrodynamic damping
- excitation: initial push / sine moment amplitude and frequency

Additionally, “Record” options:

- save ( \phi(t), \dot{\phi}(t)), damper moment, dissipation, SPH energies (if used)
- CSV/JSON output (for thesis figures)

---

## 9) Documentation structure (can be copied as the thesis table of contents)

1. **Introduction**
   - ship roll problem, damping, granular dampers concept
2. **Research question and scope**
   - which DOFs are included, what is left out and why
3. **Physical models**
   - 3.1 Ship motion (roll equation / rigid body)
   - 3.2 DEM (contact model, friction, integration)
   - 3.3 SPH (kernel function choice, pressure, viscosity, boundaries)
   - 3.4 Couplings (moments, force sums, coordinate systems)
4. **Installation of the granular damper in the ship**
   - coordinate systems, attachment, container geometry, placement principles
   - assumptions (rigid attachment, no deformations)
   - parameters and justifications
5. **Software implementation in C#**
   - architecture, performance (neighbor search), determinism
   - rendering and user interface
6. **Simulation experiments**
   - roll decay, sine wave, parameter sweep
7. **Results**
   - time responses, RAO, optimal fill ratio, location effect
8. **Discussion**
   - limitations (SPH model shortcomings, contact model simplifications)
   - future work (6DOF, better wave boundaries, validation with tank tests)
9. **Appendices**
   - parameter tables
   - pseudocode / class diagrams
   - experiment config files

---

## 10) Practical implementation path (so the project doesn't get out of hand)

**Sprint 1: 2D/3D roll + DEM damper (no water)**

- roll equation + DEM moment
- show that rolling is damped more with the damper

**Sprint 2: UI + sweeps + saving**

- you get the thesis measurement figures

**Sprint 3: SPH tank (visualization + wave excitation)**

- simple wave generation
- later pressure integration on the hull, if you have time

With this, you will almost certainly finish the thesis, even if the “full coupling” of SPH remains incomplete.
