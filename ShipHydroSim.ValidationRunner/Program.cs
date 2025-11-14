using System;
using System.Collections.Generic;
using ShipHydroSim.Validation;

namespace ShipHydroSim.ValidationRunner;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("SPH-DEM Validation Test Suite");
        Console.WriteLine("Master's Thesis - Jyväskylä University");
        Console.WriteLine($"Run Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("=" + new string('=', 60) + "\n");
        
        bool runFullTests = args.Length > 0 && args[0] == "--full";
        bool generateThesis = args.Length > 0 && (args[0] == "--thesis" || args[^1] == "--thesis");
        
        if (runFullTests)
        {
            Console.WriteLine("Running FULL validation suite (includes simulations)...\n");
            var tests = new ValidationTests();
            var report = tests.RunAllTests();
            report.PrintSummary();
            
            if (generateThesis)
            {
                var fastTests = new FastValidationTests();
                var fastResults = RunFastTestsAndGetResults(fastTests);
                ThesisReportGenerator.SaveThesisMaterials("./ThesisValidation", fastResults, report);
            }
            
            int exitCode = report.Results.All(r => r.Passed) && report.Errors.Count == 0 ? 0 : 1;
            Environment.Exit(exitCode);
        }
        else
        {
            Console.WriteLine("Running FAST analytical tests (use --full for simulation tests)...\n");
            var fastTests = new FastValidationTests();
            var results = RunFastTestsAndGetResults(fastTests);
            
            fastTests.RunAll();
            
            if (generateThesis)
            {
                Console.WriteLine("\nGenerating thesis materials...");
                ThesisReportGenerator.SaveThesisMaterials("./ThesisValidation", results);
            }
            
            Console.WriteLine("\n✓ Fast validation complete.");
            Console.WriteLine("  --full     : Run comprehensive simulation tests");
            Console.WriteLine("  --thesis   : Generate LaTeX/CSV/Python files for thesis");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
    
    static List<FastValidationTests.TestResult> RunFastTestsAndGetResults(FastValidationTests tests)
    {
        // Run tests and collect results
        return new List<FastValidationTests.TestResult>
        {
            tests.TestKernelNormalization(),
            tests.TestHydrostaticFormula(),
            tests.TestArchimedesPrinciple(),
            tests.TestTerminalVelocityFormula(),
            tests.TestWaveDispersion(),
            tests.TestNewtonThirdLaw(),
            tests.TestDragFormulaula(),
            tests.TestQuaternionNormalization()
        };
    }
}
