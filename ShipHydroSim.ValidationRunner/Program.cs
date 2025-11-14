using System;
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
        
        if (runFullTests)
        {
            Console.WriteLine("Running FULL validation suite (includes simulations)...\n");
            var tests = new ValidationTests();
            var report = tests.RunAllTests();
            report.PrintSummary();
            
            int exitCode = report.Results.All(r => r.Passed) && report.Errors.Count == 0 ? 0 : 1;
            Environment.Exit(exitCode);
        }
        else
        {
            Console.WriteLine("Running FAST analytical tests (use --full for simulation tests)...\n");
            var fastTests = new FastValidationTests();
            fastTests.RunAll();
            
            Console.WriteLine("\n✓ Fast validation complete. Use --full flag for comprehensive simulation tests.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
