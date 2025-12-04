# Scientific Simulator Project Structure (Template)

## 1. Research Question & Goals
- What phenomenon is being simulated?
- What are the key quantities to measure?
- Why is this simulation important?

## 2. Mathematical Model
- List governing equations (e.g., Navier–Stokes, DEM)
- State assumptions and simplifications
- Define all variables and parameters

## 3. Software Architecture
- Main modules/classes:
    - `TankGeometry`: tank dimensions and motion
    - `FluidModel`: SPH or CFD logic
    - `DamperModel`: DEM or particle logic
    - `Simulator`: time integration, main loop
    - `Measurements`: data collection
    - `Frontend`: visualization and UI
- Data flow diagram (how modules interact)
- API endpoints (if web-based)

## 4. Implementation & Testing
- Start with a minimal working version
- Add features incrementally
- Unit tests for each module/class
- Example input files and test cases

## 5. Documentation & Rationale
- README: project overview, setup, usage
- Inline code comments: explain logic and decisions
- Document why each method/model was chosen

## 6. Validation & Comparison
- Compare results to literature, experiments, or analytical solutions
- Error analysis and limitations

## 7. Iteration & Development
- Version control (Git): commit often, write clear messages
- Keep a development log (what was changed and why)
- Plan next steps and improvements

## 8. Publication & Sharing
- Make code easy to run and modify
- Provide sample results and figures
- Share code and findings for peer review

---

## Example Directory Structure
```
ResonanceSimulation/
├── Core/              # Simulation logic (SPH, DEM, etc.)
├── Web/               # Web server and API
├── Frontend/          # Visualization (HTML/JS)
├── Results/           # Output data and figures
├── pythonkoodit/      # Post-processing scripts
├── README.md
├── SIMULATOR_STRUCTURE.md
└── ...
```

---

## Example Class Skeleton (C#)
```csharp
public class Simulator {
    public TankGeometry Tank { get; set; }
    public FluidModel Fluid { get; set; }
    public DamperModel Damper { get; set; }
    public Measurements Data { get; set; }
    public void Initialize() { /* ... */ }
    public void Run() { /* ... */ }
}
```

---

## Example API Endpoint (Web)
```http
POST /api/start      # Start simulation with parameters
GET  /api/state      # Get current simulation state
```

---

## Example Development Log Entry
```
2025-12-04: Refactored particle update logic for better stability. Added unit test for wall pressure calculation.
```

---

This template helps you build a scientific simulator step by step, with clear structure and rationale for every decision.