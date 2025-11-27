using System;
using System.Collections.Generic;
using SPHDEM_Simulation_New.Ship;
using SPHDEM_Simulation_New.DEM;

namespace SPHDEM_Simulation_New.DEM {
    public class TankBoundaryTest {
        public static void RunTest() {
            var tank = new Tank { Width = 4.0, Height = 3.0, Length = 10.0 };
            var particles = new List<DEMParticle> {
                new DEMParticle { X = -1, Y = 1, Mass = 1, Velocity = new double[2] { 1, 0 }, Force = new double[2] }, // vasen reuna
                new DEMParticle { X = 11, Y = 2, Mass = 1, Velocity = new double[2] { -1, 0 }, Force = new double[2] }, // oikea reuna
                new DEMParticle { X = 5, Y = -2, Mass = 1, Velocity = new double[2] { 0, 1 }, Force = new double[2] }, // ala reuna
                new DEMParticle { X = 5, Y = 5, Mass = 1, Velocity = new double[2] { 0, -1 }, Force = new double[2] }  // ylä reuna
            };
            var dem = new DEMSimulator(particles) { Tank = tank };
            dem.Update();
            foreach (var p in particles) {
                Console.WriteLine($"Particle at ({p.X:F2}, {p.Y:F2}), velocity ({p.Velocity[0]:F2}, {p.Velocity[1]:F2})");
            }
        }
    }
}
