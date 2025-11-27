using System;
using System.Collections.Generic;
using SPHDEM_Simulation_New.Ship;
using SPHDEM_Simulation_New.SPH;

namespace SPHDEM_Simulation_New.SPH {
    public class TankBoundaryTest {
        public static void RunTest() {
            var tank = new Tank { Width = 4.0, Height = 3.0, Length = 10.0 };
            var particles = new List<SPHParticle> {
                new SPHParticle(-1, 1, 1), // vasen reuna
                new SPHParticle(11, 2, 1), // oikea reuna
                new SPHParticle(5, -2, 1), // ala reuna
                new SPHParticle(5, 5, 1)  // ylä reuna
            };
            var sph = new SPHSimulator(particles) { Tank = tank };
            sph.Update();
            foreach (var p in particles) {
                Console.WriteLine($"Particle at ({p.X:F2}, {p.Y:F2})");
            }
        }
    }
}
