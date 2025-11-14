# Scope, Assumptions, and Threats to Validity

This document outlines the intended scope of the project, modeling assumptions, and key threats to validity to help readers interpret results appropriately.

## Scope and Intended Use
- Research and education only; not for design certification or operational decisions.
- Illustrative 3D SPH+DEM simulations for ship–wave interaction and sloshing phenomena.
- Primary goal: explore feasibility and inform future, more rigorous studies.

## Modeling Assumptions (Non-exhaustive)
- Weakly compressible SPH with simplified EOS; limited incompressibility enforcement.
- DEM rigid body as box-approximation; simplified hydrostatics and damping.
- Idealized tank boundaries; no complex turbulence or added mass models.
- Wave models (Sine/Stokes/Irregular) parameterized; not site-calibrated.
- Coupling uses simplified momentum exchange near boundaries.

## Known Limitations
- No mesh-based hull; contact geometry simplified.
- CPU-only; resolution constrained by performance.
- Fixed spatial hash cell size; no adaptive refinement.
- No periodic boundaries; reflective walls only.
- Limited sensitivity and convergence testing; partial validation.

## Threats to Validity
- External validity: Synthetic scenarios may not represent real sea states.
- Construct validity: Simplified measures (e.g., average height/velocity) may under-represent key dynamics.
- Internal validity: Parameter interactions and numerical diffusion can confound effects.
- Conclusion validity: Small sample sizes and lack of uncertainty quantification risk overinterpretation.

## Comparison to Literature
- Benchmarks are qualitative/indicative; quantitative parity is not claimed.
- Discrepancies can arise from kernel choice, resolution, EOS, and coupling details.
- Where deviations exist, they are discussed as hypotheses, not definitive conclusions.

## Uncertainty & Reproducibility
- Random seed is fixed and stamped in run metadata.
- Core parameters (h, Δt, ρ0, ν, wave parameters) recorded for each run.
- Provide experiment manifests (`Results/EXPERIMENT_MANIFEST.yaml`) to aid reproduction.

## Recommendations for Readers
- Treat figures as exploratory evidence.
- Prefer trends over absolute values.
- Cross-check claims with cited peer-reviewed sources when available.
