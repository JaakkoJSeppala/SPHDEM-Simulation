using System;
using System.Collections.Generic;
using SPHDEM_Simulation_New.Ship;
using SPHDEM_Simulation_New.SPH;

namespace SPHDEM_Simulation_New.Ship {
    public class ShipBoundaryTest {
        public static void RunTest() {
            var ship = new Ship(0, 0) { Width = 4.0, Height = 3.0, Mass = 500 };
            var particles = new List<SPHParticle>();
            // Simuloidaan tilanne, jossa laiva on tankin yläreunassa
            ship.Y = 400 - ship.Height + 1; // Yläreunan yli
            ship.VY = 10; // Suuri nopeus ylöspäin
            ship.UpdatePhysics(particles, 0.1);
            Console.WriteLine($"Ship Y: {ship.Y}, VY: {ship.VY}");
            // Odotetaan, että laiva pysyy tankin sisällä (Y <= 400 - Height)
        }
    }
}
