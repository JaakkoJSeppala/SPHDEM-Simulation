using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ShipHydroSim.App.Models;

public static class ExperimentLogger
{
    public static void StampRun(object parameters)
    {
        try
        {
            var resultsDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Results");
            resultsDir = Path.GetFullPath(resultsDir);
            Directory.CreateDirectory(resultsDir);

            var meta = new
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0",
                environment = new
                {
                    os = Environment.OSVersion.ToString(),
                    framework = $".NET {Environment.Version}",
                    process = Environment.ProcessPath
                },
                seed = 424242,
                parameters
            };

            var json = JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true });
            var file = Path.Combine(resultsDir, $"run_metadata_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            File.WriteAllText(file, json);

            Console.WriteLine($"[Run Metadata] written: {file}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ExperimentLogger error: {ex.Message}");
        }
    }
}
