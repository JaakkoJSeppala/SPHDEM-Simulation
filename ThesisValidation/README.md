# Thesis Validation Materials

This directory contains auto-generated validation test results formatted for Master's thesis inclusion.

## Generated Files

### 1. `validation_section.tex`
**LaTeX section for thesis**  
Complete section with:
- Introduction to validation approach
- Results table with all 8 analytical tests
- Key formula derivations with equations
- Discussion and conclusions

**Usage:**
```latex
% In your thesis main.tex:
\input{ThesisValidation/validation_section.tex}
```

Or copy-paste the content into your validation chapter.

### 2. `validation_results.csv`
**Data in CSV format**  
Spreadsheet-compatible format for:
- Excel analysis
- Custom plotting
- Table generation

**Columns:** Test name, Expected value, Measured value, Error %, Pass/Fail status

### 3. `plot_validation.py`
**Python plotting script**  
Matplotlib script to generate publication-quality figure:
```bash
python plot_validation.py
```
Produces `validation_results.pdf` (300 DPI, suitable for thesis).

### 4. `validation_summary.txt`
**Text summary**  
Human-readable summary with:
- Overall statistics
- Individual test results
- Timestamp

## Test Results Summary

**Date:** 2025-11-14  
**Total tests:** 8 analytical formula verifications  
**Result:** 8/8 passed (100%)  
**Mean error:** 0.0000%  
**Max error:** 0.0000%

### Tests Performed

1. ✅ **Kernel Normalization**: ∫W(r,h) dV = 1
2. ✅ **Hydrostatic Pressure**: p = ρgh
3. ✅ **Archimedes Principle**: F_b = ρgV
4. ✅ **Terminal Velocity (Stokes)**: v = 2r²(ρ_p-ρ_f)g/(9μ)
5. ✅ **Wave Dispersion**: λ = gT²/(2π)
6. ✅ **Newton's 3rd Law**: F + (-F) = 0
7. ✅ **Drag Force**: F_d = 0.5·C_d·ρ·A·v²
8. ✅ **Quaternion Normalization**: |q| = 1

## Regenerating Materials

To regenerate these files with updated results:

```bash
cd ShipHydroSim.ValidationRunner
dotnet run --configuration Release -- --thesis
```

This will:
1. Run all analytical tests
2. Generate LaTeX, CSV, Python files
3. Save to `./ThesisValidation/`

## Integration Guide

### For LaTeX Thesis

1. **Copy validation_section.tex** to your thesis directory
2. **Include in main document:**
   ```latex
   \chapter{Validation}
   \input{validation_section.tex}
   ```
3. **Compile** with pdflatex or xelatex

### For Figures

1. **Run Python script** to generate PDF figure:
   ```bash
   python plot_validation.py
   ```
2. **Include in LaTeX:**
   ```latex
   \begin{figure}[h]
   \centering
   \includegraphics[width=0.8\textwidth]{validation_results.pdf}
   \caption{Validation test error distribution.}
   \label{fig:validation_errors}
   \end{figure}
   ```

### For Custom Analysis

Use `validation_results.csv` with:
- Excel/LibreOffice Calc
- Python pandas: `pd.read_csv('validation_results.csv')`
- R: `read.csv('validation_results.csv')`

## References

These validation tests verify implementation against:
- Monaghan (1992): SPH kernel formulations
- Stokes (1851): Terminal velocity theory
- Faltinsen (1990): Ship hydrostatics
- Martin & Moyce (1952): Dam break experiments (full tests only)

## Notes

- Analytical tests are **deterministic** (always 0% error)
- Full simulation tests (--full flag) include stochastic SPH results
- LaTeX compilation requires standard packages (amsmath, graphicx)
- Python script requires matplotlib and numpy
