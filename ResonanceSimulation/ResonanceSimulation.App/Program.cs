using CommandLine;
using Newtonsoft.Json;
using ResonanceSimulation.Core;

namespace ResonanceSimulation.App;

/// <summary>
/// Komentoriviargumentit.
/// </summary>
class Options
{
    [Option('f', "frequency", Required = false, HelpText = "Excitation frequency (Hz)")]
    public double? Frequency { get; set; }

    [Option('d', "damper", Required = false, Default = true, HelpText = "Enable granular damper")]
    public bool EnableDamper { get; set; }

    [Option('s', "sweep", Required = false, Default = false, HelpText = "Run frequency sweep")]
    public bool FrequencySweep { get; set; }

    [Option('o', "output", Required = false, Default = "results", HelpText = "Output directory")]
    public string OutputDir { get; set; } = "results";

    [Option('t', "time", Required = false, Default = 30.0, HelpText = "Simulation time (s)")]
    public double TotalTime { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Resonance Simulation: 1:50 Ballast Tank ===\n");

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunSimulation)
            .WithNotParsed(errors => Console.WriteLine("Invalid arguments. Use --help for usage."));
    }

    static void RunSimulation(Options opts)
    {
        Directory.CreateDirectory(opts.OutputDir);

        if (opts.FrequencySweep)
        {
            RunFrequencySweep(opts);
        }
        else
        {
            RunSingleSimulation(opts);
        }
    }

    /// <summary>
    /// Aja yksittäinen simulaatio.
    /// </summary>
    static void RunSingleSimulation(Options opts)
    {
        var config = new SimulationConfig
        {
            Frequency = opts.Frequency ?? 0.6,
            EnableDamper = opts.EnableDamper,
            TotalTime = opts.TotalTime
        };

        Console.WriteLine($"Configuration:");
        Console.WriteLine($"  Frequency: {config.Frequency} Hz");
        Console.WriteLine($"  Damper: {(config.EnableDamper ? "Enabled" : "Disabled")}");
        Console.WriteLine($"  Total time: {config.TotalTime} s");
        Console.WriteLine();

        var simulator = new Simulator(config);
        simulator.Initialize();
        
        var startTime = DateTime.Now;
        simulator.Run();
        var elapsed = DateTime.Now - startTime;

        Console.WriteLine($"Elapsed time: {elapsed.TotalSeconds:F1} s");
        Console.WriteLine();

        // Tallenna tulokset
        var measurements = simulator.GetMeasurements();
        string filename = $"f_{config.Frequency:F2}Hz_{(config.EnableDamper ? "damper" : "nodamper")}.csv";
        string filepath = Path.Combine(opts.OutputDir, filename);
        measurements.SaveToCSV(filepath);

        // Tulosta yhteenveto
        Console.WriteLine("Results:");
        Console.WriteLine($"  Max wall pressure: {measurements.MaxWallPressure:F1} Pa");
        Console.WriteLine($"  Max free surface height: {measurements.MaxFreeSurfaceHeight:F4} m");
        Console.WriteLine($"  Damping ratio ζ: {measurements.ComputeDampingRatio():F4}");
        Console.WriteLine($"  Saved to: {filepath}");
    }

    /// <summary>
    /// Aja taajuuspyyhkäisy (frequency sweep).
    /// </summary>
    static void RunFrequencySweep(Options opts)
    {
        // Taajuudet: 0.2...1.0 Hz, 10 pistettä
        double[] frequencies = Enumerable.Range(0, 10)
            .Select(i => 0.2 + i * 0.08)
            .ToArray();

        Console.WriteLine($"Running frequency sweep: {frequencies.Length} frequencies");
        Console.WriteLine($"Frequencies: {string.Join(", ", frequencies.Select(f => $"{f:F2} Hz"))}");
        Console.WriteLine();

        var sweepResults = new List<SweepResult>();

        foreach (double freq in frequencies)
        {
            foreach (bool enableDamper in new[] { false, true })
            {
                Console.WriteLine($"\n--- Frequency: {freq:F2} Hz, Damper: {(enableDamper ? "ON" : "OFF")} ---");

                var config = new SimulationConfig
                {
                    Frequency = freq,
                    EnableDamper = enableDamper,
                    TotalTime = opts.TotalTime
                };

                var simulator = new Simulator(config);
                simulator.Initialize();

                var startTime = DateTime.Now;
                simulator.Run();
                var elapsed = DateTime.Now - startTime;

                var measurements = simulator.GetMeasurements();

                // Tallenna yksittäinen simulaatio
                string subdir = enableDamper ? "sweep_withdamper" : "sweep_nodamper";
                Directory.CreateDirectory(Path.Combine(opts.OutputDir, subdir));
                string filename = $"f_{freq:F2}Hz.csv";
                string filepath = Path.Combine(opts.OutputDir, subdir, filename);
                measurements.SaveToCSV(filepath);

                // Tallenna yhteenvetodataa
                sweepResults.Add(new SweepResult
                {
                    Frequency = freq,
                    EnableDamper = enableDamper,
                    MaxPressure = measurements.MaxWallPressure,
                    MaxHeight = measurements.MaxFreeSurfaceHeight,
                    DampingRatio = measurements.ComputeDampingRatio(),
                    ElapsedSeconds = elapsed.TotalSeconds
                });

                Console.WriteLine($"Completed in {elapsed.TotalSeconds:F1} s");
                Console.WriteLine($"  Max pressure: {measurements.MaxWallPressure:F1} Pa");
                Console.WriteLine($"  Damping ratio: {measurements.ComputeDampingRatio():F4}");
            }
        }

        // Tallenna yhteenveto
        SaveSweepSummary(sweepResults, opts.OutputDir);
        Console.WriteLine($"\nSweep complete! Results saved to {opts.OutputDir}/");
    }

    /// <summary>
    /// Tallenna taajuuspyyhkäisyn yhteenveto (resonanssikäyrä yms.).
    /// </summary>
    static void SaveSweepSummary(List<SweepResult> results, string outputDir)
    {
        Directory.CreateDirectory(Path.Combine(outputDir, "summary"));

        // Resonanssikäyrä: frequency vs max_pressure
        string resonancePath = Path.Combine(outputDir, "summary", "resonance_curve.csv");
        using (var writer = new StreamWriter(resonancePath))
        {
            writer.WriteLine("Frequency,MaxPressure_NoDamper,MaxPressure_WithDamper");

            var grouped = results.GroupBy(r => r.Frequency);
            foreach (var group in grouped)
            {
                double freq = group.Key;
                double pNoDamper = group.FirstOrDefault(r => !r.EnableDamper)?.MaxPressure ?? 0;
                double pWithDamper = group.FirstOrDefault(r => r.EnableDamper)?.MaxPressure ?? 0;

                writer.WriteLine($"{freq:F3},{pNoDamper:F2},{pWithDamper:F2}");
            }
        }

        // Vaimennussuhde: frequency vs zeta
        string dampingPath = Path.Combine(outputDir, "summary", "damping_ratios.csv");
        using (var writer = new StreamWriter(dampingPath))
        {
            writer.WriteLine("Frequency,DampingRatio_NoDamper,DampingRatio_WithDamper");

            var grouped = results.GroupBy(r => r.Frequency);
            foreach (var group in grouped)
            {
                double freq = group.Key;
                double zetaNoDamper = group.FirstOrDefault(r => !r.EnableDamper)?.DampingRatio ?? 0;
                double zetaWithDamper = group.FirstOrDefault(r => r.EnableDamper)?.DampingRatio ?? 0;

                writer.WriteLine($"{freq:F3},{zetaNoDamper:F6},{zetaWithDamper:F6}");
            }
        }

        // JSON yhteenveto
        string jsonPath = Path.Combine(outputDir, "summary", "sweep_summary.json");
        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(results, Formatting.Indented));

        Console.WriteLine($"Summary saved:");
        Console.WriteLine($"  - {resonancePath}");
        Console.WriteLine($"  - {dampingPath}");
        Console.WriteLine($"  - {jsonPath}");
    }
}

/// <summary>
/// Taajuuspyyhkäisyn yhden simulaation tulos.
/// </summary>
class SweepResult
{
    public double Frequency { get; set; }
    public bool EnableDamper { get; set; }
    public double MaxPressure { get; set; }
    public double MaxHeight { get; set; }
    public double DampingRatio { get; set; }
    public double ElapsedSeconds { get; set; }
}
