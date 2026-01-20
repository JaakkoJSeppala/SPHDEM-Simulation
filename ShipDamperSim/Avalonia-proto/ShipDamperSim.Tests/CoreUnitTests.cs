using NUnit.Framework;
using ShipDamperSim.Core;
using System.IO;

namespace ShipDamperSim.Tests
{
    /// <summary>
    /// Yksikkötestit ydinkomponenteille: parametrien luku, partikkelien luonti, simulaation eteneminen.
    /// </summary>
    [TestFixture]
    public class CoreUnitTests
    {
        /// <summary>
        /// Testaa, että parametrit latautuvat jsonista oikein ja sisältävät järkevät arvot.
        /// </summary>
        [Test]
        public void SimulationParameters_LoadsFromJson()
        {
            var parameters = SimulationParameters.LoadFromJson("../../../../config.json");
            Assert.IsNotNull(parameters);
            Assert.Greater(parameters.Tank.Length, 0);
            Assert.Greater(parameters.Sph.Resolution, 0);
        }

        /// <summary>
        /// Testaa, että SPH-malli luo neste- ja boundary-partikkelit.
        /// </summary>
        [Test]
        public void SphFluid_CreatesParticles()
        {
            var parameters = SimulationParameters.LoadFromJson("../../../../config.json");
            var sph = new SphFluid(parameters);
            Assert.IsTrue(sph.Particles.Count > 0);
            Assert.IsTrue(sph.Boundaries.Count > 0);
        }

        /// <summary>
        /// Testaa, että DEM-damperi luo rakeet.
        /// </summary>
        [Test]
        public void DemDamper_CreatesParticles()
        {
            var parameters = SimulationParameters.LoadFromJson("../../../../config.json");
            var dem = new DemDamper(parameters);
            Assert.IsTrue(dem.Particles.Count > 0);
        }

        /// <summary>
        /// Testaa, että simulaation askel kasvattaa aikaa.
        /// </summary>
        [Test]
        public void Simulation_StepAdvancesTime()
        {
            var parameters = SimulationParameters.LoadFromJson("../../../../config.json");
            var sim = new Simulation(parameters);
            double t0 = sim.Time;
            sim.Step(0.01);
            Assert.Greater(sim.Time, t0);
        }
    }
}
