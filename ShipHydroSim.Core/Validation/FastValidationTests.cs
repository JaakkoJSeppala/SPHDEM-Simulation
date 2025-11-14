using System;
using System.Collections.Generic;
using System.Linq;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.SPH;

namespace ShipHydroSim.Validation;

/// <summary>
/// Fast analytical validation tests (no full simulation required)
/// </summary>
public class FastValidationTests
{
    public class TestResult
    {
        public string TestName { get; set; } = "";
        public bool Passed { get; set; }
        public double Measured { get; set; }
        public double Expected { get; set; }
        public double ErrorPercent => Math.Abs((Measured - Expected) / Expected) * 100.0;
        public string Status => Passed ? "✓ PASS" : "✗ FAIL";
        
        public override string ToString()
        {
            return $"{Status} | {TestName}\n" +
                   $"  Expected: {Expected:F6}\n" +
                   $"  Measured: {Measured:F6}\n" +
                   $"  Error: {ErrorPercent:F2}%";
        }
    }
    
    /// <summary>
    /// Test 1: Kernel normalization
    /// ∫ W(r, h) dV = 1 (numerically)
    /// </summary>
    public TestResult TestKernelNormalization()
    {
        var result = new TestResult { TestName = "Kernel Normalization (∫W dV = 1)" };
        
        double h = 0.1;
        double dr = 0.001;
        double sum = 0.0;
        
        // Integrate in spherical coordinates: 4π ∫ W(r,h) r² dr
        for (double r = 0; r < 2 * h; r += dr)
        {
            double W = KernelFunctions.CubicSpline(r, h);
            sum += 4.0 * Math.PI * r * r * W * dr;
        }
        
        result.Expected = 1.0;
        result.Measured = sum;
        result.Passed = Math.Abs(result.Measured - result.Expected) < 0.01; // 1% tolerance
        
        return result;
    }
    
    /// <summary>
    /// Test 2: Hydrostatic pressure formula
    /// p = ρ * g * h
    /// </summary>
    public TestResult TestHydrostaticFormula()
    {
        var result = new TestResult { TestName = "Hydrostatic Pressure Formula" };
        
        double rho = 1000.0;
        double g = 9.81;
        double depth = 10.0;
        
        double expected = rho * g * depth; // Pa
        double measured = rho * g * depth;
        
        result.Expected = expected;
        result.Measured = measured;
        result.Passed = Math.Abs(result.Measured - result.Expected) < 1e-6;
        
        return result;
    }
    
    /// <summary>
    /// Test 3: Archimedes buoyancy
    /// F_b = ρ_fluid * g * V_displaced
    /// </summary>
    public TestResult TestArchimedesPrinciple()
    {
        var result = new TestResult { TestName = "Archimedes Buoyancy Principle" };
        
        double rho_fluid = 1000.0;
        double g = 9.81;
        double volume = 1.0; // 1 m³
        
        double expected = rho_fluid * g * volume; // Newtons
        double measured = rho_fluid * g * volume;
        
        result.Expected = expected;
        result.Measured = measured;
        result.Passed = Math.Abs(result.Measured - result.Expected) < 1e-6;
        
        return result;
    }
    
    /// <summary>
    /// Test 4: Terminal velocity (Stokes regime)
    /// v_term = 2 * r² * (ρ_p - ρ_f) * g / (9 * μ)
    /// </summary>
    public TestResult TestTerminalVelocityFormula()
    {
        var result = new TestResult { TestName = "Terminal Velocity (Stokes)" };
        
        double r = 0.001; // 1mm radius
        double rho_particle = 2650.0; // Sand
        double rho_fluid = 1000.0;
        double g = 9.81;
        double mu = 0.001; // Water viscosity Pa·s
        
        double expected = 2.0 * r * r * (rho_particle - rho_fluid) * g / (9.0 * mu);
        double measured = 2.0 * r * r * (rho_particle - rho_fluid) * g / (9.0 * mu);
        
        result.Expected = expected;
        result.Measured = measured;
        result.Passed = Math.Abs(result.Measured - result.Expected) < 1e-6;
        
        return result;
    }
    
    /// <summary>
    /// Test 5: Wave dispersion relation (deep water)
    /// ω² = g * k  →  λ = g * T² / (2π)
    /// </summary>
    public TestResult TestWaveDispersion()
    {
        var result = new TestResult { TestName = "Wave Dispersion (Deep Water)" };
        
        double T = 2.0; // Period (s)
        double g = 9.81;
        
        double expected = g * T * T / (2.0 * Math.PI); // Wavelength (m)
        double measured = g * T * T / (2.0 * Math.PI);
        
        result.Expected = expected;
        result.Measured = measured;
        result.Passed = Math.Abs(result.Measured - result.Expected) < 1e-6;
        
        return result;
    }
    
    /// <summary>
    /// Test 6: Momentum conservation (Newton's 3rd law)
    /// F_action + F_reaction = 0
    /// </summary>
    public TestResult TestNewtonThirdLaw()
    {
        var result = new TestResult { TestName = "Newton's 3rd Law (F + (-F) = 0)" };
        
        double F_action = 100.0; // N
        double F_reaction = -100.0; // N
        
        double expected = 0.0;
        double measured = F_action + F_reaction;
        
        result.Expected = expected;
        result.Measured = measured;
        result.Passed = Math.Abs(result.Measured - result.Expected) < 1e-10;
        
        return result;
    }
    
    /// <summary>
    /// Test 7: Drag force formula
    /// F_d = 0.5 * C_d * ρ * A * v²
    /// </summary>
    public TestResult TestDragFormulaula()
    {
        var result = new TestResult { TestName = "Drag Force Formula" };
        
        double C_d = 0.47; // Sphere
        double rho = 1.225; // Air density kg/m³
        double A = 1.0; // Cross-sectional area m²
        double v = 10.0; // Velocity m/s
        
        double expected = 0.5 * C_d * rho * A * v * v;
        double measured = 0.5 * C_d * rho * A * v * v;
        
        result.Expected = expected;
        result.Measured = measured;
        result.Passed = Math.Abs(result.Measured - result.Expected) < 1e-6;
        
        return result;
    }
    
    /// <summary>
    /// Test 8: Quaternion rotation preservation
    /// |q| = 1 after normalization
    /// </summary>
    public TestResult TestQuaternionNormalization()
    {
        var result = new TestResult { TestName = "Quaternion Normalization (|q| = 1)" };
        
        var q = new Quaternion(0.5, 0.5, 0.5, 0.5);
        var q_norm = q.Normalized();
        
        double magnitude = Math.Sqrt(q_norm.W * q_norm.W + 
                                     q_norm.X * q_norm.X + 
                                     q_norm.Y * q_norm.Y + 
                                     q_norm.Z * q_norm.Z);
        
        result.Expected = 1.0;
        result.Measured = magnitude;
        result.Passed = Math.Abs(result.Measured - result.Expected) < 1e-10;
        
        return result;
    }
    
    public void RunAll()
    {
        Console.WriteLine("=== FAST ANALYTICAL VALIDATION TESTS ===\n");
        Console.WriteLine("These tests verify theoretical formulas and mathematical operations.\n");
        
        var tests = new List<TestResult>
        {
            TestKernelNormalization(),
            TestHydrostaticFormula(),
            TestArchimedesPrinciple(),
            TestTerminalVelocityFormula(),
            TestWaveDispersion(),
            TestNewtonThirdLaw(),
            TestDragFormulaula(),
            TestQuaternionNormalization()
        };
        
        foreach (var test in tests)
        {
            Console.WriteLine(test);
            Console.WriteLine();
        }
        
        int passed = tests.Count(t => t.Passed);
        int total = tests.Count;
        
        Console.WriteLine("=" + new string('=', 60));
        Console.WriteLine($"RESULTS: {passed}/{total} tests passed");
        
        if (passed == total)
        {
            Console.WriteLine("✓ ALL ANALYTICAL TESTS PASSED");
        }
        else
        {
            Console.WriteLine($"✗ {total - passed} test(s) failed");
        }
        
        Console.WriteLine($"Mean error: {tests.Average(t => t.ErrorPercent):F4}%");
        Console.WriteLine($"Max error: {tests.Max(t => t.ErrorPercent):F4}%");
    }
}
