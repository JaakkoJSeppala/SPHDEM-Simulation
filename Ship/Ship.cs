using System;
using System.Collections.Generic;
using SPHDEM_Simulation_New.SPH;

namespace SPHDEM_Simulation_New.Ship {
    public class Ship {
        public double X { get; set; }
        public double Y { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public double Width { get; set; } = 80;
        public double Height { get; set; } = 30;
        public double Mass { get; set; } = 500;
        public double Buoyancy { get; private set; }
        public double Drag { get; private set; }

        public Ship(double x, double y) {
            X = x;
            Y = y;
            VX = 0;
            VY = 0;
        }

        public void UpdatePhysics(List<SPHParticle> particles, double dt) {
            // Painovoima
            double gravity = Mass * 9.81;
            // Nosteen laskenta: laivan upotettu tilavuus
            double waterDensity = 1000.0; // kg/m^3
            double submergedHeight = Math.Min(Height, Y + Height); // upotettu korkeus
            double submergedFraction = submergedHeight / Height;
            double volumeSubmerged = Width * submergedHeight;
            Buoyancy = waterDensity * volumeSubmerged * 9.81 * 0.7; // noste < painovoima
            // Drag (yksinkertainen)
            Drag = -VY * 10.0;
            // Net force
            double netForce = Buoyancy - gravity + Drag;
            // Integrointi
            VY += netForce / Mass * dt;
            Y += VY * dt;
            // Reunaehdot
            if (Y < 0) { Y = 0; VY = 0; }
            if (Y > 400 - Height) { Y = 400 - Height; VY = 0; }
        }
    }
}
