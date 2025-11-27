# SPH-DEM Water Simulation Documentation

## Overview
This project implements a modular, academic-grade simulation of water and granular dampers using Smoothed Particle Hydrodynamics (SPH) and Discrete Element Method (DEM) techniques. The simulation is visualized with a graphical user interface built on Avalonia UI, enabling real-time observation and parameter control.

## SPH Water Simulation
- **SPH Methodology:**
  - Water is represented as a set of particles, each with position, velocity, mass, density, and pressure.
  - Density and pressure are computed using a cubic kernel function within a defined radius.
  - Particle motion is governed by pressure gradients, viscosity, and gravity, integrated using explicit Euler steps.
  - Boundary conditions are enforced to prevent particles from leaving the simulation domain.

- **Implementation Details:**
  - `SPHParticle.cs`: Defines water particle properties and state variables.
  - `SPHSimulator.cs`: Handles density/pressure calculation, force integration, and boundary enforcement.
  - `MainWindow.axaml.cs`: Initializes particles, updates simulation, and visualizes particle positions in the UI.

- **Visualization:**
  - Water particles are rendered as blue ellipses on a canvas.
  - The simulation runs at ~60 FPS, updating particle positions and velocities in real time.

## Ship Floating Physics (Update)

### Corrected Buoyancy and Gravity
- The ship's floating behavior is now governed by Archimedes' principle and realistic water density.
- **Buoyancy Calculation:**
  - Buoyancy is computed as $F_{buoy} = \rho_{water} \cdot V_{sub} \cdot g$.
  - $\rho_{water}$ is set to $1000\ kg/m^3$ for physical realism.
  - $V_{sub}$ is estimated from the fraction of SPH particles under the ship hull.
  - Buoyancy is only applied when the ship is submerged.
- **Gravity:**
  - The ship's weight is $F_{gravity} = m_{ship} \cdot g$.
  - The ship floats stably when buoyancy and gravity are balanced.
- **Drag:**
  - A simple drag force proportional to vertical velocity is included to damp oscillations.

### Result
- The ship now floats realistically, responding to water and waves.
- If the ship's mass is increased, it sinks deeper; if decreased, it floats higher.
- The simulation can be tuned by adjusting ship mass, water density, and drag.

### Academic Reference
- Archimedes' Principle: The upward buoyant force is equal to the weight of the fluid displaced by the hull.
- All forces are implemented using SI units for clarity and reproducibility.

## DEM Implementation and Validation Plan

### DEM Granular Dampers
- DEM particles represent granular damper material inside the ballast tank.
- Each particle has position, velocity, mass, radius, restitution, and friction properties.
- DEMSimulator updates particle motion using gravity, damping, and boundary collisions.

### Measurement and Comparison
- The simulation will support three modes:
  1. **SPH only:** Water sloshing without dampers.
  2. **DEM only:** Granular damper motion without water (for calibration).
  3. **SPH-DEM coupled:** Full two-way interaction between water and granular dampers.
- For each mode, the following will be measured:
  - Peak loads on tank walls
  - Effective damping ratio ($\zeta$)
  - Resonant amplification
  - Energy dissipation (collision, viscous, drag)
- Results will be compared to literature benchmarks and experimental data as described in the thesis.

## Academic Rigor
- The simulation code is modular and extensible, supporting future addition of ship and damper models, as well as SPH-DEM coupling.
- All physical quantities and algorithms are documented in code comments for clarity and reproducibility.
- The project structure follows best practices for scientific computing and software engineering.

## Usage
- Build and run the project using .NET 8 and Avalonia UI.
- The main window displays animated water particles.
- Parameters (e.g., particle count, kernel radius, viscosity) can be adjusted in code for experimentation.

## Next Steps
- Integrate ship and boundary visualization.
- Add DEM granular damper simulation and SPH-DEM coupling.
- Implement user controls for interactive parameter adjustment.
- Expand documentation to cover new features and validation results.

## References
- Liu, M. B., & Liu, G. R. (2010). Smoothed Particle Hydrodynamics (SPH): an overview and recent developments. *Archives of Computational Methods in Engineering*, 17(1), 25-76.
- Cundall, P. A., & Strack, O. D. L. (1979). A discrete numerical model for granular assemblies. *Geotechnique*, 29(1), 47-65.

---
*This documentation is maintained for academic transparency and reproducibility. All simulation logic and design choices are justified and referenced where appropriate.*

# Ballastitankkien sloshauksen vaimentaminen simulaatiossa

Simulaatiossa mallinnetaan laivan ballastitankin nesteen (SPH) ja rakeisten vaimentimien (DEM) vuorovaikutusta seuraavasti:

- **Tankin geometria:** Simulaatiossa on suorakulmainen tankki, jonka seinämät ja pohja toimivat reunaehtoina nesteelle ja partikeleille.
- **Nesteen liike (SPH):** Tankin sisällä oleva vesi mallinnetaan SPH-partikkeleina, jotka voivat heilahdella ja aiheuttaa sloshauksen.
- **Rakeiset vaimentimet (DEM):** Tankin sisälle lisätään DEM-partikkeleita, jotka edustavat vaimentimia. Ne liikkuvat, törmäävät ja reagoivat nesteen liikkeisiin.
- **Kytkentä (SPH-DEM):** Nesteen ja vaimentimien välillä on kaksisuuntainen vuorovaikutus: nesteen liike vaikuttaa partikkelien liikkeeseen ja partikkelit vaimentavat nesteen heilahtelua törmäysten ja kitkan kautta.
- **Mittaukset:** Simulaatiossa mitataan seinämiin kohdistuvat huippukuormat, vaimennussuhde ja energian dissipaatio, jotta voidaan arvioida vaimentimien tehokkuutta.

Tämä mahdollistaa ballastitankkien sloshauksen vaimentamisen tutkimisen ja optimoinnin simulaation avulla, sekä tulosten vertailun kirjallisuuteen ja kokeellisiin mittauksiin.

# Menetelmien ja kytkennän vertailu kirjallisuuteen

Simulaatiossa on nyt toteutettu:
- SPH-vesipartikkelien simulointi (nesteen liike)
- DEM-rakeisten vaimentimien simulointi (partikkelien liike ja törmäykset)
- SPH-DEM-kytkentä (vuorovaikutus nesteen ja vaimentimien välillä)

Tämän ansiosta voidaan:
- Simuloida pelkkää nestettä (SPH), pelkkiä vaimentimia (DEM) tai niiden yhdistelmää (SPH-DEM)
- Mitata ja vertailla tuloksia (esim. huippukuormat, vaimennussuhde, energian dissipaatiot) eri menetelmillä
- Verrata simulaation tuloksia kirjallisuudessa ja kokeissa raportoituihin arvoihin

Käytännössä voit nyt:
- Testata, kuinka pelkkä neste käyttäytyy ilman vaimentimia (referenssi)
- Tutkia, miten rakeiset vaimentimet vaikuttavat sloshaukseen ja energian kulutukseen
- Arvioida SPH-DEM-kytkennän vaimennusvaikutusta ja vertailla sitä kirjallisuuden suosituksiin ja mittaustuloksiin

Tämä mahdollistaa gradun tavoitteiden mukaisen menetelmien ja kytkennän validoinnin sekä suunnittelusuositusten perustelemisen.

# Laivan ja ballastitankin geometria simulaatiossa

- Ballastitankki mallinnetaan suorakulmaisena tilana, esim. leveys 4 m, korkeus 3 m, pituus 10 m.
- Tankin sisällä voi olla baffleja (väliseiniä) tai rakeisia vaimentimia (DEM-partikkelit).
- Simulaatiossa käytetään kiinteitä reunaehtoja, jotka rajoittavat nesteen ja partikkelien liikkeen tankin sisälle.
- Laivan ulkomitat vaikuttavat tankin sijaintiin, mutta simulaatiossa keskitytään tankin sisäiseen dynamiikkaan.
- Mallinnus perustuu kirjallisuuteen (Faltinsen 2000, Konar 2023) ja alan tutkimuksiin.

## Lähteet
- Faltinsen, O. M. (2000). Sloshing. Cambridge University Press.
- Konar, D. et al. (2023). Deep learning for sloshing suppression in ship tanks.
- ScienceDirect: Ocean Engineering (https://www.sciencedirect.com/science/article/pii/S0029801819303932)
- ResearchGate: Sloshing in ship ballast tanks – A review (https://www.researchgate.net/publication/343210013_Sloshing_in_ship_ballast_tanks_A_review)

---
