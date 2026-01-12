# WP3: Results Analysis and Reporting

## 1. Simulation Results Analysis

The SPH simulation was run with 100 particles in a 2D box under gravity. The following statistical and mathematical analyses were performed:

- **Mean particle height:** 0.43
- **Standard deviation of height:** 0.46
- **Normal fit to heights:** mean ≈ 0.43, std ≈ 0.46

The distribution of particle heights is close to normal, as shown by the histogram and normal fit. Most particles settle towards the bottom of the domain due to gravity, as expected.

The simulated pressure profile as a function of height was compared to the theoretical hydrostatic pressure $p(y) = \rho_0 g (y_0 - y)$. The simulated and theoretical profiles are in good agreement, confirming the physical correctness of the model.

## 2. Visualization

- **Histogram of particle heights:**
  - Shows the distribution of particles after settling. The histogram matches a normal distribution centered near the bottom of the domain.
- **Pressure profile plot:**
  - Simulated pressure values plotted against particle height.
  - Theoretical hydrostatic pressure curve plotted for comparison.
  - The two curves are close, indicating the simulation reproduces expected hydrostatic behavior.

## 3. Interpretation and Discussion

The results support theoretical expectations:

- Particles settle under gravity, forming a denser region at the bottom.
- The pressure increases with depth, matching the hydrostatic law.
- The statistical properties (mean, std) of the particle heights are consistent with the expected distribution for a system under gravity.

Possible sources of error or deviation include:

- Limited number of particles (discretization error)
- Boundary effects due to simple wall conditions
- Short simulation time (system may not reach full equilibrium)

## 4. Reporting

The findings confirm that the implemented SPH model can reproduce basic hydrostatic equilibrium in 2D. Figures (histogram and pressure profile) illustrate the main results and can be included in the final report.
