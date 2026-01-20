using System;
using System.Collections.Generic;

namespace ShipDamperSim
{
    /// <summary>
    /// Simple SPH (Smoothed Particle Hydrodynamics) fluid simulation for water.
    /// </summary>
    public class SphFluid
    {
        private double _simTime = 0.0;

        public class Particle
        {
            public Vec3 Position;
            public Vec3 Velocity;
            public Vec3 Force;
            public double Density;
            public double Pressure;
        }

        public List<Particle> Particles = new();
        public double RestDensity = 1000.0; // Water density (kg/m^3)
        public double Viscosity = 2.0;      // Viscosity coefficient (suurempi -> vähemmän brownin liikettä)
        public double GasConstant = 12000.0; // Stiffness of the fluid (jäykempi neste)
        public double ParticleMass = 0.01;  // Pienempi hiukkasmassa -> tiheämpi neste
        public double SmoothingLength = 0.035; // Kernel radius (pienempi, tiheämpi verkko)
        public Vec3 Gravity = new(0, -9.81, 0);

        // Kernel constants
        private double Poly6, SpikyGrad, ViscLap;

        private double _waveAmplitude = 0.3;
        private double _waveFrequency = 0.5;
        public void SetWaveParams(double amplitude, double frequency)
        {
            _waveAmplitude = amplitude;
            _waveFrequency = frequency;
        }

        public SphFluid(int particleCount)
        {
            // Precompute kernel constants
            double h = SmoothingLength;
            Poly6 = 315.0 / (64.0 * Math.PI * Math.Pow(h, 9));
            SpikyGrad = -45.0 / (Math.PI * Math.Pow(h, 6));
            ViscLap = 45.0 / (Math.PI * Math.Pow(h, 6));

            // Lisää hiukkasmäärää (esim. 500–2000)
            int n = (int)Math.Ceiling(Math.Pow(Math.Max(particleCount, 1000), 1.0 / 3.0));
            double spacing = h * 1.0; // harvempi verkko, nopeampi
            // Vesi alustetaan tyynenä laatikkona: X [-12,12], Z [-3,3], Y [0,0.7]
            double minX = -12.0, maxX = 12.0;
            double minY = 0.0, maxY = 0.7;
            double minZ = -3.0, maxZ = 3.0;
            int nx = (int)((maxX - minX) / spacing);
            int ny = (int)((maxY - minY) / spacing);
            int nz = (int)((maxZ - minZ) / spacing);
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                    for (int z = 0; z < nz; z++)
                        if (Particles.Count < Math.Max(particleCount, 8000))
                        {
                            double px = minX + x * spacing;
                            double py = minY + y * spacing;
                            double pz = minZ + z * spacing;
                            Particles.Add(new Particle
                            {
                                Position = new Vec3(px, py, pz),
                                Velocity = new Vec3(0, 0, 0),
                                Force = new Vec3(0, 0, 0),
                                Density = RestDensity,
                                Pressure = 0
                            });
                        }

            // Debug: tulosta ensimmäisten 10 hiukkasen sijainnit
            Console.WriteLine($"SPH debug: nx={nx}, ny={ny}, nz={nz}, spacing={spacing:F3}, minX={minX}, maxX={maxX}");
            for (int i = 0; i < Math.Min(10, Particles.Count); i++)
            {
                var p = Particles[i];
                Console.WriteLine($"Particle {i}: x={p.Position.X:F2}, y={p.Position.Y:F2}, z={p.Position.Z:F2}");
            }
        }

        /// <summary>
        /// Perform one simulation step.
        /// </summary>

        public void Step(double dt)
        {
            // 1. Compute density and pressure
            foreach (var pi in Particles)
            {
                pi.Density = 0;
                foreach (var pj in Particles)
                {
                    var rij = pi.Position - pj.Position;
                    double r2 = rij.Norm2();
                    double h2 = SmoothingLength * SmoothingLength;
                    if (r2 < h2)
                        pi.Density += ParticleMass * Poly6 * Math.Pow(h2 - r2, 3);
                }
                pi.Pressure = GasConstant * (pi.Density - RestDensity);
            }

            // 2. Compute forces
            foreach (var pi in Particles)
            {
                Vec3 fPressure = new(0, 0, 0);
                Vec3 fViscosity = new(0, 0, 0);
                foreach (var pj in Particles)
                {
                    if (pi == pj) continue;
                    var rij = pi.Position - pj.Position;
                    double r = rij.Norm();
                    if (r < SmoothingLength && r > 1e-6)
                    {
                        // Pressure force
                        double avgP = (pi.Pressure + pj.Pressure) / 2.0;
                        fPressure += -rij.Normalized() * ParticleMass * avgP / pj.Density * SpikyGrad * Math.Pow(SmoothingLength - r, 2);
                        // Viscosity force
                        fViscosity += Viscosity * ParticleMass * (pj.Velocity - pi.Velocity) / pj.Density * ViscLap * (SmoothingLength - r);
                    }
                }
                // Gravity
                Vec3 fGravity = Gravity * pi.Density;
                pi.Force = fPressure + fViscosity + fGravity;
            }

            // 3. Integrate
            // Define box boundaries (wider for full screen)
            // Add moving left wall (minX) for wave generation
            double t = _simTime;
            double wallOsc = _waveAmplitude * Math.Sin(2 * Math.PI * _waveFrequency * t);
            double minX = -4.0 + wallOsc, maxX = 4.0;
            double minY = 0, maxY = 2.0;
            double minZ = -3.0, maxZ = 3.0;
            foreach (var p in Particles)
            {
                p.Velocity += dt * p.Force / p.Density;
                p.Position += dt * p.Velocity;
                // Boundary: floor
                if (p.Position.Y < minY)
                {
                    p.Position = new Vec3(p.Position.X, minY, p.Position.Z);
                    p.Velocity = new Vec3(p.Velocity.X, p.Velocity.Y * -0.5, p.Velocity.Z);
                }
                // Boundary: ceiling
                if (p.Position.Y > maxY)
                {
                    p.Position = new Vec3(p.Position.X, maxY, p.Position.Z);
                    p.Velocity = new Vec3(p.Velocity.X, p.Velocity.Y * -0.5, p.Velocity.Z);
                }
                // Boundary: left/right
                if (p.Position.X < minX)
                {
                    p.Position = new Vec3(minX, p.Position.Y, p.Position.Z);
                    p.Velocity = new Vec3(p.Velocity.X * -0.5, p.Velocity.Y, p.Velocity.Z);
                }
                if (p.Position.X > maxX)
                {
                    p.Position = new Vec3(maxX, p.Position.Y, p.Position.Z);
                    p.Velocity = new Vec3(p.Velocity.X * -0.5, p.Velocity.Y, p.Velocity.Z);
                }
                // Boundary: front/back
                if (p.Position.Z < minZ)
                {
                    p.Position = new Vec3(p.Position.X, p.Position.Y, minZ);
                    p.Velocity = new Vec3(p.Velocity.X, p.Velocity.Y, p.Velocity.Z * -0.5);
                }
                if (p.Position.Z > maxZ)
                {
                    p.Position = new Vec3(p.Position.X, p.Position.Y, maxZ);
                    p.Velocity = new Vec3(p.Velocity.X, p.Velocity.Y, p.Velocity.Z * -0.5);
                }
            }
            _simTime += dt;
        }
    }
}
