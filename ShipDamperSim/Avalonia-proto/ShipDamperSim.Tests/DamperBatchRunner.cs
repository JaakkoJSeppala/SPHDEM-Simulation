using System;
using System.Collections.Generic;
using System.IO;
using ShipDamperSim.Core;

namespace ShipDamperSim.Tests
{
    public static class DamperBatchRunner
    {
        public static void RunAll()
        {
            float dt = 0.01f;
            int steps = 5000;
            float[] fillRatios = { 0.0f, 0.3f, 0.6f };
            for (int test = 0; test < fillRatios.Length; test++)
            {
                var sim = new DamperSimulator();
                // Init particles based on fill ratio
                int n = (int)(fillRatios[test] * 50); // 50 max
                sim.Dem.Particles.Clear();
                for (int i = 0; i < n; i++)
                {
                    sim.Dem.Particles.Add(new Particle
                    {
                        Position = new System.Numerics.Vector2(0.1f * i, 0),
                        Velocity = System.Numerics.Vector2.Zero,
                        Radius = 0.05f,
                        Mass = 1.0f
                    });
                }
                // Alusta roll decay (alkukulma, nolla nopeus)
                sim.Roll.Angle = 0.2f; // rad
                sim.Roll.AngularVelocity = 0f;
                // Simuloi
                for (int i = 0; i < steps; i++)
                    sim.Step(dt);
                // Vie CSV
                string name = fillRatios[test] == 0.0f ? "damper_off" : $"damper_on_{(int)(fillRatios[test] * 100)}";
                string path = Path.Combine("Results", $"{name}.csv");
                Directory.CreateDirectory("Results");
                CsvExporter.WriteEnergies(path, sim.TimeHistory, sim.ShipKinHistory, sim.GranKinHistory, sim.DissHistory);
            }
        }
    }
}
