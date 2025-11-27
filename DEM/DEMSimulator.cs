using System.Collections.Generic;
using System;
using SPHDEM_Simulation_New.Ship;

namespace SPHDEM_Simulation_New.DEM {
    public class DEMSimulator {
                // Laske turbulent kinetic energy (TKE) partikkelien nopeuksista
                public double CalculateTKE() {
                    double meanVX = 0, meanVY = 0;
                    int n = Particles.Count;
                    foreach (var p in Particles) {
                        meanVX += p.Velocity[0];
                        meanVY += p.Velocity[1];
                    }
                    meanVX /= n;
                    meanVY /= n;
                    double sumSqVX = 0, sumSqVY = 0;
                    foreach (var p in Particles) {
                        sumSqVX += Math.Pow(p.Velocity[0] - meanVX, 2);
                        sumSqVY += Math.Pow(p.Velocity[1] - meanVY, 2);
                    }
                    double tke = 0.5 * ((sumSqVX + sumSqVY) / n);
                    return tke;
                }
        public List<DEMParticle> Particles { get; private set; }
        public double TimeStep { get; set; } = 0.01;
        public double Restitution { get; set; } = 0.5;
        public double Friction { get; set; } = 0.3;
        public Tank Tank { get; set; } = new Tank(); // Ballastitankin malli

        public DEMSimulator(List<DEMParticle> particles) {
            Particles = particles;
        }

        public void Update() {
            // Simple DEM: gravity, collision with boundaries, and damping
            foreach (var p in Particles) {
                // Gravity
                p.Force[1] = -p.Mass * 9.81;
                // Damping
                p.Force[0] += -0.1 * p.Velocity[0];
                p.Force[1] += -0.1 * p.Velocity[1];
            }
            // Integrate
            foreach (var p in Particles) {
                for (int i = 0; i < 2; i++) {
                    p.Velocity[i] += p.Force[i] / p.Mass * TimeStep;
                }
                p.X += p.Velocity[0] * TimeStep;
                p.Y += p.Velocity[1] * TimeStep;
                // Boundary collision (simple)
                if (p.Y < 0) { p.Y = 0; p.Velocity[1] *= -Restitution; }
                // Reunaehdot: Faltinsen 2000, luku 2.2
                if (!Tank.IsInside(p.X, p.Y)) {
                    (p.X, p.Y) = Tank.EnforceBoundaries(p.X, p.Y, Restitution);
                    p.Velocity[0] *= -Restitution;
                    p.Velocity[1] *= -Restitution;
                }
            }
        }
    }
}
