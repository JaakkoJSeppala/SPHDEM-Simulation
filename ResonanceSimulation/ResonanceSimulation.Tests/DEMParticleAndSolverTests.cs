using System;
using System.Collections.Generic;
using Xunit;
using ResonanceSimulation.Core;

namespace ResonanceSimulation.Tests.DEM
{
    public class DEMParticleTests
    {
        [Fact]
        public void Constructor_SetsPhysicalPropertiesCorrectly()
        {
            var pos = new Vector2D(1.0, 2.0);
            double radius = 0.5;
            double density = 2000.0;
            double E = 1e7;
            double nu = 0.3;
            double e = 0.5;
            double mu = 0.3;

            var p = new DEMParticle(pos, radius, density, E, nu, e, mu);

            Assert.Equal(pos, p.Position);
            Assert.Equal(radius, p.Radius);
            Assert.Equal(E, p.YoungsModulus);
            Assert.Equal(nu, p.PoissonRatio);
            Assert.Equal(e, p.RestitutionCoeff);
            Assert.Equal(mu, p.FrictionCoeff);
            Assert.True(p.Mass > 0);
            Assert.True(p.Inertia > 0);
        }

        [Fact]
        public void ResetForces_SetsAllForcesAndAccelerationsToZero()
        {
            var p = new DEMParticle(new Vector2D(0,0), 0.5, 1000);
            p.ContactForce = new Vector2D(1,2);
            p.FluidForce = new Vector2D(3,4);
            p.ContactTorque = 5.0;
            p.Acceleration = new Vector2D(1,1);
            p.AngularAcceleration = 2.0;

            p.ResetForces();

            Assert.Equal(Vector2D.Zero, p.ContactForce);
            Assert.Equal(Vector2D.Zero, p.FluidForce);
            Assert.Equal(0.0, p.ContactTorque);
            Assert.Equal(Vector2D.Zero, p.Acceleration);
            Assert.Equal(0.0, p.AngularAcceleration);
        }

        [Fact]
        public void UpdateAcceleration_ComputesCorrectValues()
        {
            var p = new DEMParticle(new Vector2D(0,0), 0.5, 1000);
            p.ContactForce = new Vector2D(2,0);
            p.FluidForce = new Vector2D(0,3);
            p.ContactTorque = 4.0;
            var gravity = new Vector2D(0, -9.81);

            double expectedMass = p.Mass;
            Vector2D totalForce = p.ContactForce + p.FluidForce + expectedMass * gravity;
            Vector2D expectedAcc = totalForce / expectedMass;
            double expectedAlpha = p.ContactTorque / p.Inertia;

            p.UpdateAcceleration(gravity);

            Assert.Equal(expectedAcc.X, p.Acceleration.X, 6);
            Assert.Equal(expectedAcc.Y, p.Acceleration.Y, 6);
            Assert.Equal(expectedAlpha, p.AngularAcceleration, 6);
        }
    }

    public class DEMSolverTests
    {
        [Fact]
        public void ComputeContacts_NoOverlap_NoForce()
        {
            var config = new SimulationConfig();
            var solver = new DEMSolver(config);
            var p1 = new DEMParticle(new Vector2D(0,0), 0.5, 1000);
            var p2 = new DEMParticle(new Vector2D(2,0), 0.5, 1000);
            var particles = new List<DEMParticle> { p1, p2 };

            solver.ComputeContacts(particles);

            Assert.Equal(Vector2D.Zero, p1.ContactForce);
            Assert.Equal(Vector2D.Zero, p2.ContactForce);
        }

        [Fact]
        public void ComputeContacts_Overlap_ForceApplied()
        {
            var config = new SimulationConfig { TimeStep = 1e-4 };
            var solver = new DEMSolver(config);
            var p1 = new DEMParticle(new Vector2D(0,0), 0.5, 1000);
            var p2 = new DEMParticle(new Vector2D(0.9,0), 0.5, 1000);
            var particles = new List<DEMParticle> { p1, p2 };

            solver.ComputeContacts(particles);

            // Forces should be equal and opposite
            Assert.True(p1.ContactForce.Length() > 0);
            Assert.True(p2.ContactForce.Length() > 0);
            Assert.Equal(p1.ContactForce, -p2.ContactForce);
        }
    }
}
