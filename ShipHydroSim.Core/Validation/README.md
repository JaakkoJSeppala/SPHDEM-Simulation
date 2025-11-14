# Validation Test Suite

## Overview

This directory contains validation tests for the SPH-DEM coupling implementation. Tests verify both analytical formulas and numerical simulation accuracy against established theory and experimental data.

## Test Types

### 1. Fast Analytical Tests (`FastValidationTests.cs`)
**Runtime**: < 1 second  
**Purpose**: Verify mathematical correctness of fundamental formulas

Tests included:
- ✅ **Kernel Normalization**: ∫ W(r, h) dV = 1
- ✅ **Hydrostatic Pressure**: p = ρgh
- ✅ **Archimedes Principle**: F_b = ρ_fluid · g · V_displaced
- ✅ **Terminal Velocity (Stokes)**: v_term = 2r²(ρ_p - ρ_f)g / (9μ)
- ✅ **Wave Dispersion**: λ = gT² / (2π) for deep water
- ✅ **Newton's 3rd Law**: F_action + F_reaction = 0
- ✅ **Drag Force**: F_d = 0.5 · C_d · ρ · A · v²
- ✅ **Quaternion Normalization**: |q| = 1

**Run command**:
```bash
dotnet run --project ShipHydroSim.ValidationRunner
```

### 2. Full Simulation Tests (`ValidationTests.cs`)
**Runtime**: 5-10 minutes  
**Purpose**: Quantitative validation against experimental/analytical benchmarks

Tests included:
1. **Single Particle Sedimentation**
   - Measures terminal velocity
   - Compares to: v_terminal = √(2mg/(ρAC_d))
   - Tolerance: 5%

2. **Hydrostatic Pressure Distribution**
   - Samples pressure at mid-depth
   - Compares to: p(y) = ρg(h - y)
   - Tolerance: 10% (SPH pressure is noisy)

3. **Dam Break Surge Front**
   - Measures surge front position
   - Compares to: Martin & Moyce (1952) experimental data
   - Tolerance: 15% (complex phenomenon)

4. **Wave Generation Accuracy**
   - Measures wave amplitude and period
   - Compares to input parameters
   - Tolerance: 5%

5. **Floating Body Equilibrium**
   - Measures draft of floating box
   - Compares to: Archimedes principle
   - Tolerance: 10%

**Run command**:
```bash
dotnet run --project ShipHydroSim.ValidationRunner -- --full
```

## Test Results

### Fast Analytical Tests (Latest Run: 2025-11-14)
```
RESULTS: 8/8 tests passed
✓ ALL ANALYTICAL TESTS PASSED
Mean error: 0.0000%
Max error: 0.0000%
```

All fundamental formulas verified correct.

### Full Simulation Tests
Status: **Implementation complete, awaiting full run**

Expected outcomes:
- SPH pressure field accuracy: ~5-10% error (acceptable for weakly compressible SPH)
- Dam break comparison: ~10-15% error (complex free-surface flow)
- Wave generation: ~5% error (linear theory)
- Floating equilibrium: ~10% error (coupling accuracy)

## Pass/Fail Criteria

Each test has predefined tolerance based on:
1. **Theoretical tests** (analytical): < 1% error
2. **SPH field tests**: < 10% error (inherent SPH approximation)
3. **Complex phenomena** (dam break): < 15% error

Tests use automatic pass/fail evaluation:
```csharp
result.Passed = (measuredError / expected) < tolerance;
```

## Integration with Thesis

These validation tests demonstrate:

1. **Correctness of implementation**: Mathematical formulas match theory
2. **Numerical accuracy**: SPH-DEM coupling produces physically reasonable results
3. **Quantitative benchmarks**: Comparison to established experimental/analytical data
4. **Reproducibility**: Automated tests can be re-run to verify consistency

## References

- Martin & Moyce (1952): "Part IV. An experimental study of the collapse of liquid columns on a rigid horizontal plane"
- Monaghan (1992): "Smoothed Particle Hydrodynamics" (kernel formulations)
- Stokes (1851): Terminal velocity in viscous fluids
- Faltinsen (1990): "Sea Loads on Ships and Offshore Structures" (hydrostatics)

## Future Improvements

- [ ] Energy conservation test (monitor E_kin + E_pot drift)
- [ ] Convergence analysis (vary particle resolution)
- [ ] Reynolds number correction for drag
- [ ] Pressure smoothing analysis (noise reduction)
- [ ] Wave reflection coefficient measurement
