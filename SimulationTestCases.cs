using System;
using System.Collections.Generic;

namespace SPHDEM_Simulation_New {
    public static class SimulationTestCases {
        public class TestCase {
            public string Name { get; set; } = "";
            public double TankLength { get; set; }
            public double TankHeight { get; set; }
            public double WaterHeight { get; set; }
            public double ShipMass { get; set; }
            public double WaveFrequency { get; set; }
            public double WaveAmplitude { get; set; }
        }

        public static List<TestCase> GetDefaultCases() => new List<TestCase> {
            new TestCase { Name = "Perus", TankLength = 600, TankHeight = 200, WaterHeight = 60, ShipMass = 500, WaveFrequency = 1, WaveAmplitude = 20 },
            new TestCase { Name = "Syvä vesi", TankLength = 600, TankHeight = 200, WaterHeight = 150, ShipMass = 500, WaveFrequency = 1, WaveAmplitude = 20 },
            new TestCase { Name = "Kevyt laiva", TankLength = 600, TankHeight = 200, WaterHeight = 60, ShipMass = 200, WaveFrequency = 1, WaveAmplitude = 20 },
            new TestCase { Name = "Vaimennus", TankLength = 600, TankHeight = 200, WaterHeight = 60, ShipMass = 500, WaveFrequency = 1, WaveAmplitude = 60 },
            new TestCase { Name = "Korkea taajuus", TankLength = 600, TankHeight = 200, WaterHeight = 60, ShipMass = 500, WaveFrequency = 3, WaveAmplitude = 20 },
            new TestCase { Name = "Artikkeli_SC", TankLength = 500, TankHeight = 25, WaterHeight = 25, ShipMass = 500, WaveFrequency = 1, WaveAmplitude = 20 },
            new TestCase { Name = "Artikkeli_USC", TankLength = 500, TankHeight = 25, WaterHeight = 25, ShipMass = 500, WaveFrequency = 1, WaveAmplitude = 20 }
        };
    }
}
