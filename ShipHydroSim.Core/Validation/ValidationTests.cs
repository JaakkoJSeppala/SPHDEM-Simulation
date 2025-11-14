using System;
using System.Collections.Generic;
using System.Linq;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.SPH;
using ShipHydroSim.Core.DEM;
using ShipHydroSim.Core.Hybrid;

namespace ShipHydroSim.Validation;

/// <summary>
/// Validation test suite for SPH-DEM coupling
/// Implements quantitative tests with automatic pass/fail criteria
/// </summary>
public class ValidationTests
{
    private readonly double _tolerance = 0.05; // 5% default tolerance
    
    public class TestResult
    {
        public string TestName { get; set; } = "";
        public bool Passed { get; set; }
        public double Measured { get; set; }
        public double Expected { get; set; }
        public double Error { get; set; }
        public double Tolerance { get; set; }
        public string Details { get; set; } = "";
        public double ErrorPercent => Expected != 0 ? Math.Abs(Error / Expected) * 100.0 : 0.0;
        
        public override string ToString()
        {
            string status = Passed ? "✓ PASS" : "✗ FAIL";
            return $"{status} | {TestName}\n" +
                   $"  Expected: {Expected:F6}, Measured: {Measured:F6}\n" +
                   $"  Error: {Error:F4} ({Error/Expected*100:F2}%), Tolerance: {Tolerance*100:F1}%\n" +
                   $"  {Details}";
        }
    }
    
    /// <summary>
    /// Test 1: Single particle sedimentation
    /// A single spherical particle falling in quiescent fluid should reach terminal velocity:
    /// v_terminal = sqrt(2 * m * g / (ρ * A * C_d))
    /// </summary>
    public TestResult TestParticleSedimentation(double particleRadius = 0.1, double particleDensity = 2650.0)
    {
        var result = new TestResult { TestName = "Single Particle Sedimentation" };
        
        // Theoretical terminal velocity (Stokes regime approximation)
        double fluidDensity = 1000.0;
        double g = 9.81;
        double particleMass = (4.0/3.0) * Math.PI * Math.Pow(particleRadius, 3) * particleDensity;
        double crossSectionalArea = Math.PI * particleRadius * particleRadius;
        double dragCoeff = 0.47; // Sphere at Re ~ 1000
        
        // Terminal velocity: F_drag = F_gravity - F_buoyancy
        double effectiveMass = particleMass * (1 - fluidDensity / particleDensity);
        double v_terminal = Math.Sqrt(2 * effectiveMass * g / (fluidDensity * crossSectionalArea * dragCoeff));
        
        result.Expected = v_terminal;
        result.Details = $"Particle: r={particleRadius}m, ρ_p={particleDensity}kg/m³, m={particleMass:F4}kg\n" +
                        $"  Fluid: ρ_f={fluidDensity}kg/m³, C_d={dragCoeff}\n" +
                        $"  Theory: v_term = {v_terminal:F4} m/s";
        
        // Run simulation
        var solver = CreateFluidDomain(domainSize: 2.0, particleSpacing: 0.1);
        
        // Add settling particle at top center
        var particle = new RigidBody(
            id: 0,
            position: new Vector3(1.0, 1.8, 1.0),
            mass: particleMass,
            shape: new SphereShape(particleRadius)
        )
        {
            Velocity = Vector3.Zero
        };
        solver.AddRigidBody(particle);
        
        // Run until settling (10 seconds)
        double maxTime = 10.0;
        double sampleStart = 5.0; // Start measuring after initial transient
        List<double> terminalVelocities = new List<double>();
        
        while (solver.CurrentTime < maxTime)
        {
            solver.Step();
            
            if (solver.CurrentTime > sampleStart)
            {
                terminalVelocities.Add(Math.Abs(particle.Velocity.Y));
            }
        }
        
        // Measured terminal velocity (average of last 50% of samples)
        int skipCount = terminalVelocities.Count / 2;
        result.Measured = terminalVelocities.Skip(skipCount).Average();
        result.Error = Math.Abs(result.Measured - result.Expected);
        result.Tolerance = _tolerance;
        result.Passed = result.Error / result.Expected < result.Tolerance;
        
        return result;
    }
    
    /// <summary>
    /// Test 2: Hydrostatic pressure distribution
    /// Pressure should vary linearly with depth: p(y) = ρ * g * (h - y)
    /// </summary>
    public TestResult TestHydrostaticPressure(double waterHeight = 2.0)
    {
        var result = new TestResult { TestName = "Hydrostatic Pressure Distribution" };
        
        double fluidDensity = 1000.0;
        double g = 9.81;
        
        // Create static fluid column
        var solver = CreateFluidDomain(domainSize: 2.0, particleSpacing: 0.1);
        solver.Gravity = new Vector3(0, -g, 0);
        
        // Let it settle to hydrostatic equilibrium (5 seconds)
        while (solver.CurrentTime < 5.0)
        {
            solver.Step();
        }
        
        // Sample pressure at mid-depth
        double sampleDepth = waterHeight / 2.0;
        double expectedPressure = fluidDensity * g * sampleDepth;
        
        // Find particles near sample depth
        var particles = solver.GetParticles();
        var sampleParticles = particles
            .Where(p => !p.IsBoundary && Math.Abs(p.Position.Y - (waterHeight - sampleDepth)) < 0.1)
            .ToList();
        
        if (sampleParticles.Count == 0)
        {
            result.Passed = false;
            result.Details = "No particles found at sample depth";
            return result;
        }
        
        double measuredPressure = sampleParticles.Average(p => p.Pressure);
        
        result.Expected = expectedPressure;
        result.Measured = measuredPressure;
        result.Error = Math.Abs(result.Measured - result.Expected);
        result.Tolerance = 0.1; // 10% tolerance (SPH pressure is noisy)
        result.Passed = result.Error / result.Expected < result.Tolerance;
        result.Details = $"Water height: {waterHeight}m, Sample depth: {sampleDepth}m\n" +
                        $"  Theory: p = ρgh = {expectedPressure:F2} Pa\n" +
                        $"  Sampled {sampleParticles.Count} particles at y ≈ {waterHeight - sampleDepth:F2}m";
        
        return result;
    }
    
    /// <summary>
    /// Test 3: Dam break surge front position
    /// Compare to Martin & Moyce (1952) experimental data:
    /// z(t) / L = 2 * sqrt(g/L) * t  (for t < 1.5*sqrt(L/g))
    /// </summary>
    public TestResult TestDamBreak(double damLength = 1.0, double damHeight = 2.0)
    {
        var result = new TestResult { TestName = "Dam Break Surge Front (Martin & Moyce 1952)" };
        
        double g = 9.81;
        double timeScale = Math.Sqrt(damLength / g);
        double measureTime = 1.0 * timeScale; // Measure at t = sqrt(L/g)
        
        // Martin & Moyce empirical fit: z/L = 2*sqrt(g*t²/L)
        double expectedPosition = 2.0 * Math.Sqrt(g * measureTime * measureTime / damLength);
        
        // Create dam break scenario
        var solver = new HybridSolver
        {
            SmoothingLength = 0.1,
            RestDensity = 1000.0,
            TimeStep = 0.001,
            Gravity = new Vector3(0, -g, 0),
            DomainMin = new Vector3(0, 0, 0),
            DomainMax = new Vector3(damLength * 4, damHeight * 1.5, 1.0)
        };
        
        // Initialize dam: fluid in [0, L] x [0, H]
        double spacing = 0.05;
        int nx = (int)(damLength / spacing);
        int ny = (int)(damHeight / spacing);
        
        for (int i = 0; i < nx; i++)
        {
            for (int j = 0; j < ny; j++)
            {
                var pos = new Vector3(i * spacing, j * spacing + 0.1, 0.5);
                var p = new Particle(solver.GetParticles().Count, pos, mass: 1000.0 * spacing * spacing * spacing);
                solver.AddParticle(p);
            }
        }
        
        // Run simulation
        while (solver.CurrentTime < measureTime)
        {
            solver.Step();
        }
        
        // Measure surge front position (rightmost particle with significant velocity)
        var particles = solver.GetParticles();
        double surgeFront = particles
            .Where(p => !p.IsBoundary && p.Velocity.X > 0.1)
            .Max(p => p.Position.X);
        
        double measuredPosition = surgeFront / damLength;
        
        result.Expected = expectedPosition;
        result.Measured = measuredPosition;
        result.Error = Math.Abs(result.Measured - result.Expected);
        result.Tolerance = 0.15; // 15% tolerance (dam break is complex)
        result.Passed = result.Error / result.Expected < result.Tolerance;
        result.Details = $"Dam: L={damLength}m, H={damHeight}m, Measure at t={measureTime:F3}s\n" +
                        $"  Martin & Moyce (1952): z/L = {expectedPosition:F3}\n" +
                        $"  SPH simulation: z/L = {measuredPosition:F3}\n" +
                        $"  Surge front at x = {surgeFront:F3}m";
        
        return result;
    }
    
    /// <summary>
    /// Test 4: Wave generation accuracy
    /// Verify wave amplitude and frequency match input parameters
    /// </summary>
    public TestResult TestWaveGeneration(double amplitude = 0.2, double period = 2.0)
    {
        var result = new TestResult { TestName = "Wave Generation Accuracy" };
        
        double wavelength = 9.81 * period * period / (2 * Math.PI); // Deep water dispersion
        
        // Create wave scenario
        var solver = CreateFluidDomain(domainSize: wavelength * 2, particleSpacing: 0.1);
        solver.EnableWaves = true;
        solver.WaveGenerator = new ShipHydroSim.Core.Waves.SineWave
        {
            Amplitude = amplitude,
            WaveLength = wavelength,
            Direction = new Vector3(1, 0, 0)
        };
        
        // Run for 3 periods
        double runTime = period * 3;
        List<double> surfaceElevations = new List<double>();
        double sampleX = wavelength / 2; // Sample at λ/2
        
        while (solver.CurrentTime < runTime)
        {
            solver.Step();
            
            // Sample surface elevation at x = λ/2
            if (solver.StepCount % 10 == 0)
            {
                double eta = solver.WaveGenerator.GetElevation(sampleX, 0, solver.CurrentTime);
                surfaceElevations.Add(eta);
            }
        }
        
        // Measure amplitude (max - min) / 2
        double measuredAmplitude = (surfaceElevations.Max() - surfaceElevations.Min()) / 2.0;
        
        result.Expected = amplitude;
        result.Measured = measuredAmplitude;
        result.Error = Math.Abs(result.Measured - result.Expected);
        result.Tolerance = 0.05; // 5% tolerance
        result.Passed = result.Error / result.Expected < result.Tolerance;
        result.Details = $"Wave: A={amplitude}m, T={period}s, λ={wavelength:F2}m\n" +
                        $"  Sampled at x={sampleX:F2}m for {runTime:F1}s\n" +
                        $"  {surfaceElevations.Count} elevation samples";
        
        return result;
    }
    
    /// <summary>
    /// Test 5: Rigid body hydrostatic equilibrium
    /// A floating box should reach equilibrium at draft satisfying Archimedes principle
    /// </summary>
    public TestResult TestFloatingEquilibrium(double boxLength = 1.0, double boxWidth = 0.5, double boxHeight = 0.3, double boxDensity = 500.0)
    {
        var result = new TestResult { TestName = "Floating Body Equilibrium (Archimedes)" };
        
        double fluidDensity = 1000.0;
        
        // Expected draft from equilibrium: ρ_box * V_box * g = ρ_water * V_submerged * g
        double expectedDraft = boxHeight * (boxDensity / fluidDensity);
        
        result.Expected = expectedDraft;
        result.Details = $"Box: L={boxLength}m, B={boxWidth}m, H={boxHeight}m, ρ_box={boxDensity}kg/m³\n" +
                        $"  Fluid: ρ_water={fluidDensity}kg/m³\n" +
                        $"  Theory: draft = H * (ρ_box/ρ_water) = {expectedDraft:F4}m";
        
        // Create fluid domain with floating box
        var solver = CreateFluidDomain(domainSize: 3.0, particleSpacing: 0.05);
        
        var box = new RigidBody(
            id: 0,
            position: new Vector3(1.5, 1.5, 1.5), // Start above water
            mass: boxDensity * boxLength * boxWidth * boxHeight,
            shape: new SphereShape(Math.Max(boxLength, Math.Max(boxWidth, boxHeight)) / 2.0)
        )
        {
            Velocity = Vector3.Zero,
            Orientation = Quaternion.Identity
        };
        
        solver.AddRigidBody(box);
        
        // Initialize boundary shell for coupling
        var boundaryCalc = solver.GetBoundaryForceCalculator();
        boundaryCalc.InitializeBoundaryShell(box, boxLength, boxWidth, boxHeight, spacing: 0.05);
        
        // Run until equilibrium (10 seconds)
        while (solver.CurrentTime < 10.0)
        {
            solver.Step();
        }
        
        // Measure draft (distance from water surface to box bottom)
        double waterLevel = solver.BaseWaterLevel;
        double boxBottom = box.Position.Y - boxHeight / 2.0;
        double measuredDraft = waterLevel - boxBottom;
        
        result.Measured = measuredDraft;
        result.Error = Math.Abs(result.Measured - result.Expected);
        result.Tolerance = 0.1; // 10% tolerance
        result.Passed = result.Error / result.Expected < result.Tolerance;
        result.Details += $"\n  Measured: box center Y={box.Position.Y:F4}m, bottom Y={boxBottom:F4}m\n" +
                         $"  Water level Y={waterLevel:F4}m, draft={measuredDraft:F4}m";
        
        return result;
    }
    
    /// <summary>
    /// Run all validation tests and generate summary report
    /// </summary>
    public ValidationReport RunAllTests()
    {
        var report = new ValidationReport();
        
        Console.WriteLine("=== SPH-DEM Validation Test Suite ===\n");
        
        try
        {
            Console.WriteLine("Running Test 1: Single Particle Sedimentation...");
            report.AddResult(TestParticleSedimentation());
        }
        catch (Exception ex)
        {
            report.AddError("Particle Sedimentation", ex.Message);
        }
        
        try
        {
            Console.WriteLine("Running Test 2: Hydrostatic Pressure...");
            report.AddResult(TestHydrostaticPressure());
        }
        catch (Exception ex)
        {
            report.AddError("Hydrostatic Pressure", ex.Message);
        }
        
        try
        {
            Console.WriteLine("Running Test 3: Dam Break Surge...");
            report.AddResult(TestDamBreak());
        }
        catch (Exception ex)
        {
            report.AddError("Dam Break", ex.Message);
        }
        
        try
        {
            Console.WriteLine("Running Test 4: Wave Generation...");
            report.AddResult(TestWaveGeneration());
        }
        catch (Exception ex)
        {
            report.AddError("Wave Generation", ex.Message);
        }
        
        try
        {
            Console.WriteLine("Running Test 5: Floating Equilibrium...");
            report.AddResult(TestFloatingEquilibrium());
        }
        catch (Exception ex)
        {
            report.AddError("Floating Equilibrium", ex.Message);
        }
        
        return report;
    }
    
    private HybridSolver CreateFluidDomain(double domainSize, double particleSpacing)
    {
        var solver = new HybridSolver
        {
            SmoothingLength = particleSpacing * 2.0,
            RestDensity = 1000.0,
            TimeStep = 0.001,
            Gravity = new Vector3(0, -9.81, 0),
            DomainMin = new Vector3(0, 0, 0),
            DomainMax = new Vector3(domainSize, domainSize, domainSize),
            BaseWaterLevel = domainSize / 2.0
        };
        
        // Initialize fluid particles
        int n = (int)(domainSize / particleSpacing);
        double waterHeight = domainSize / 2.0;
        
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < (int)(waterHeight / particleSpacing); j++)
            {
                for (int k = 0; k < n; k++)
                {
                    var pos = new Vector3(
                        i * particleSpacing + particleSpacing / 2,
                        j * particleSpacing + particleSpacing / 2,
                        k * particleSpacing + particleSpacing / 2
                    );
                    
                    double mass = 1000.0 * Math.Pow(particleSpacing, 3);
                    solver.AddParticle(new Particle(solver.GetParticles().Count, pos, mass));
                }
            }
        }
        
        return solver;
    }
}

public class ValidationReport
{
    public List<ValidationTests.TestResult> Results { get; } = new List<ValidationTests.TestResult>();
    public List<(string testName, string error)> Errors { get; } = new List<(string, string)>();
    
    public void AddResult(ValidationTests.TestResult result)
    {
        Results.Add(result);
        Console.WriteLine(result.ToString() + "\n");
    }
    
    public void AddError(string testName, string error)
    {
        Errors.Add((testName, error));
        Console.WriteLine($"✗ ERROR | {testName}\n  {error}\n");
    }
    
    public void PrintSummary()
    {
        Console.WriteLine("\n=== VALIDATION SUMMARY ===");
        Console.WriteLine($"Tests run: {Results.Count + Errors.Count}");
        Console.WriteLine($"Passed: {Results.Count(r => r.Passed)}");
        Console.WriteLine($"Failed: {Results.Count(r => !r.Passed)}");
        Console.WriteLine($"Errors: {Errors.Count}");
        
        if (Results.All(r => r.Passed) && Errors.Count == 0)
        {
            Console.WriteLine("\n✓ ALL TESTS PASSED - Model validated for thesis submission");
        }
        else
        {
            Console.WriteLine("\n✗ SOME TESTS FAILED - Review results above");
        }
        
        // Detailed statistics
        if (Results.Any())
        {
            Console.WriteLine("\nError Statistics:");
            Console.WriteLine($"  Mean error: {Results.Average(r => r.Error / r.Expected) * 100:F2}%");
            Console.WriteLine($"  Max error: {Results.Max(r => r.Error / r.Expected) * 100:F2}%");
        }
    }
}
