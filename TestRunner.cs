using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using SPHDEM_Simulation_New.SPH;
using SPHDEM_Simulation_New.DEM;
using SPHDEM_Simulation_New.Ship;
using SPHDEM_Simulation_New.Coupling;
using System.Collections.Generic;

namespace SPHDEM_Simulation_New {
    public class TestRunner {
        public async Task RunTestCasesAsync() {
            try {
                var cases = SimulationTestCases.GetDefaultCases();
                foreach (var testCase in cases) {
                    // Alusta partikkelit ja simulaattorit
                    var particles = new List<SPHParticle>();
                    var demParticles = new List<DEMParticle>();
                    var rand = new Random();
                    double tankLength = testCase.TankLength;
                    double tankHeight = testCase.TankHeight;
                    double waterHeight = testCase.WaterHeight;
                    double shipMass = testCase.ShipMass;
                    double waveFreq = testCase.WaveFrequency;
                    double waveAmp = testCase.WaveAmplitude;
                    double tankX = 0.0;
                    double tankY = 0.0;
                    int ParticleCount = 100;
                    double CanvasHeight = 400;
                    double CanvasWidth = 760;
                    // SPH tank
                    var tank = new Tank {
                        Height = tankHeight,
                        Y = tankY,
                        Length = tankLength,
                        X = tankX
                    };
                    var simulator = new SPHSimulator(particles) { Tank = tank };
                    for (int i = 0; i < ParticleCount; i++) {
                        double x = tankX + rand.NextDouble() * tankLength;
                        double y = tankY + rand.NextDouble() * waterHeight;
                        particles.Add(new SPHParticle(x, y, 1.0));
                    }
                    // DEM
                    int demCount = 30;
                    double demRadius = 6;
                    double demMass = 2.0;
                    for (int i = 0; i < demCount; i++) {
                        double x = CanvasWidth / 2 + (i % 10) * demRadius * 2 - 30;
                        double y = CanvasHeight * 0.5 + (i / 10) * demRadius * 2;
                        demParticles.Add(new DEMParticle { X = x, Y = y, Mass = demMass, Radius = demRadius });
                    }
                    var demSimulator = new DEMSimulator(demParticles) { Tank = tank };
                    var coupler = new SPHDEMCoupler();
                    var ship = new SPHDEM_Simulation_New.Ship.Ship(tankLength / 2, waterHeight - 30) { Mass = shipMass };
                    // Mittarit
                    double elapsedTime = 0;
                    double maxWaterLevel = double.MinValue;
                    double minWaterLevel = double.MaxValue;
                    double maxShipY = double.MinValue;
                    double minShipY = double.MaxValue;
                    double initialShipVY = 0;
                    double initialShipKE = 0;
                    bool metricsInitialized = false;
                    using (var resultsWriter = new StreamWriter($"results_{testCase.Name}.csv", false)) {
                        resultsWriter.WriteLine("Time,ShipY,WaterLevel,ParticleCount,Damping,SloshingAmplitude,ShipTrajectory,EnergyTransfer,TKE_SPH,TKE_DEM");
                        for (int i = 0; i < 1000; i++) {
                            // Simulaatioaskel
                            simulator.Update();
                            demSimulator.Update();
                            coupler.ApplyCoupling(particles, demParticles);
                            elapsedTime += 0.016;
                            // Aallot
                            foreach (var p in particles) {
                                p.Y += waveAmp * Math.Sin(2 * Math.PI * waveFreq * elapsedTime + p.X / 80.0) * 0.01;
                            }
                            ship.UpdatePhysics(particles, 0.016);
                            // Mittarit
                            double waterLevel = particles.Count > 0 ? particles[0].Y : 0;
                            double shipVY = ship.VY;
                            if (!metricsInitialized) {
                                initialShipVY = shipVY;
                                initialShipKE = 0.5 * ship.Mass * shipVY * shipVY;
                                metricsInitialized = true;
                            }
                            if (waterLevel > maxWaterLevel) maxWaterLevel = waterLevel;
                            if (waterLevel < minWaterLevel) minWaterLevel = waterLevel;
                            if (ship.Y > maxShipY) maxShipY = ship.Y;
                            if (ship.Y < minShipY) minShipY = ship.Y;
                            double damping = initialShipVY != 0 ? Math.Abs(shipVY / initialShipVY) : 0;
                            double sloshingAmplitude = maxWaterLevel - minWaterLevel;
                            double shipTrajectory = maxShipY - minShipY;
                            double currentKE = 0.5 * ship.Mass * shipVY * shipVY;
                            double energyTransfer = initialShipKE != 0 ? currentKE / initialShipKE : 0;
                            double tke_sph = simulator.CalculateTKE();
                            double tke_dem = demSimulator.CalculateTKE();
                            resultsWriter.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                "{0:F3},{1:F2},{2:F2},{3},{4:F3},{5:F2},{6:F2},{7:F2},{8:F4},{9:F4}",
                                elapsedTime, ship.Y, waterLevel, particles.Count,
                                damping, sloshingAmplitude, shipTrajectory, energyTransfer,
                                tke_sph, tke_dem));
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Virhe testitapausten ajossa: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
