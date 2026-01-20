using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ShipDamperSim
{
    /// <summary>
    /// Runs parameter sweep experiments for the granular damper.
    /// </summary>
    public static class ExperimentRunner
    {
        /// <summary>
        /// Runs a harmonic excitation (sine wave) experiment and saves results.
        /// </summary>
        public static void HarmonicExcitation(SimConfig baseConfig, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            var cfg = baseConfig.DeepClone();
            cfg.Excitation.Type = "sine";
            cfg.Output.OutputDir = outputDir;
            cfg.Output.CsvFile = "harmonic_excitation.csv";
            // Save parameters
            var paramPath = Path.Combine(outputDir, "harmonic_excitation_params.json");
            File.WriteAllText(paramPath, System.Text.Json.JsonSerializer.Serialize(cfg, SimConfig.JsonOptions));
            var sim = new Simulation(cfg);
            sim.Run();
            Console.WriteLine($"Harmonic excitation experiment complete. Results in {outputDir}/harmonic_excitation.csv");
        }

        /// <summary>
        /// Runs a roll decay experiment (no waves, initial push) and saves results.
        /// </summary>
        public static void RollDecay(SimConfig baseConfig, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            var cfg = baseConfig.DeepClone();
            cfg.Excitation.Type = "none";
            cfg.Output.OutputDir = outputDir;
            cfg.Output.CsvFile = "roll_decay.csv";
            // Save parameters
            var paramPath = Path.Combine(outputDir, "roll_decay_params.json");
            File.WriteAllText(paramPath, System.Text.Json.JsonSerializer.Serialize(cfg, SimConfig.JsonOptions));
            var sim = new Simulation(cfg);
            sim.Run();
            Console.WriteLine($"Roll decay experiment complete. Results in {outputDir}/roll_decay.csv");
        }

        /// <summary>
        /// Runs parameter sweep experiments for the granular damper.
        /// </summary>
        public static void SweepFillRatio(
            SimConfig baseConfig,
            double[] fillRatios,
            string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            int idx = 0;
            foreach (var fill in fillRatios)
            {
                var cfg = baseConfig.DeepClone();
                cfg.Damper.FillRatio = fill;
                cfg.Output.OutputDir = outputDir;
                cfg.Output.CsvFile = $"sweep_fill_{idx}_fill{fill:F2}.csv";
                // Save parameters for this run
                var paramPath = Path.Combine(outputDir, $"sweep_fill_{idx}_fill{fill:F2}_params.json");
                File.WriteAllText(paramPath, System.Text.Json.JsonSerializer.Serialize(cfg, SimConfig.JsonOptions));
                var sim = new Simulation(cfg);
                sim.Run();
                idx++;
            }
        }
    }
}
