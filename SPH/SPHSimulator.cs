using System.Collections.Generic;
using SPHDEM_Simulation_New.Ship;

namespace SPHDEM_Simulation_New.SPH {
    public class SPHSimulator {
                // Laske turbulent kinetic energy (TKE) partikkelien nopeuksista
                public double CalculateTKE() {
                    double meanVX = 0, meanVY = 0;
                    int n = Particles.Count;
                    foreach (var p in Particles) {
                        meanVX += p.VX;
                        meanVY += p.VY;
                    }
                    meanVX /= n;
                    meanVY /= n;
                    double sumSqVX = 0, sumSqVY = 0;
                    foreach (var p in Particles) {
                        sumSqVX += System.Math.Pow(p.VX - meanVX, 2);
                        sumSqVY += System.Math.Pow(p.VY - meanVY, 2);
                    }
                    double tke = 0.5 * ((sumSqVX + sumSqVY) / n);
                    return tke;
                }
        public List<SPHParticle> Particles { get; private set; }
        public double TimeStep { get; set; } = 0.01;
        public double Viscosity { get; set; } = 0.1;
        public double KernelRadius { get; set; } = 10.0;
        public Tank Tank { get; set; } = new Tank(); // Ballastitankin malli

        public SPHSimulator(List<SPHParticle> particles) {
            Particles = particles;
        }

        // Simple update: Euler integration for demonstration
        public void Update() {
            // Step 1: Compute density and pressure
            foreach (var pi in Particles) {
                double density = 0;
                foreach (var pj in Particles) {
                    double dx = pi.X - pj.X;
                    double dy = pi.Y - pj.Y;
                    double r2 = dx * dx + dy * dy;
                    double h2 = KernelRadius * KernelRadius;
                    if (r2 < h2) {
                        double q = 1.0 - (System.Math.Sqrt(r2) / KernelRadius);
                        density += pj.Mass * q * q * q;
                    }
                }
                pi.Density = density;
                pi.Pressure = System.Math.Max(0, density - 1.0); // Simple EOS
            }

            // Step 2: Compute forces and integrate
            foreach (var pi in Particles) {
                double fx = 0, fy = 0;
                foreach (var pj in Particles) {
                    if (pi == pj) continue;
                    double dx = pi.X - pj.X;
                    double dy = pi.Y - pj.Y;
                    double r2 = dx * dx + dy * dy;
                    double h2 = KernelRadius * KernelRadius;
                    if (r2 < h2 && r2 > 1e-6) {
                        double r = System.Math.Sqrt(r2);
                        double q = 1.0 - (r / KernelRadius);
                        double pressureTerm = -(pi.Pressure + pj.Pressure) / (2 * pj.Density);
                        fx += pressureTerm * q * dx / r;
                        fy += pressureTerm * q * dy / r;
                    }
                }
                // Add gravity
                fy += pi.Mass * 9.81;
                // Integrate velocity
                pi.VX += fx / pi.Density * TimeStep;
                pi.VY += fy / pi.Density * TimeStep;
            }

            // Step 3: Integrate position
            foreach (var p in Particles) {
                p.X += p.VX * TimeStep;
                p.Y += p.VY * TimeStep;
                // Reunaehdot: Faltinsen 2000, luku 2.2
                if (!Tank.IsInside(p.X, p.Y)) {
                    (p.X, p.Y) = Tank.EnforceBoundaries(p.X, p.Y, 0.5);
                    p.VX *= -0.5;
                    p.VY *= -0.5;
                }
            }
        }
    }
}
