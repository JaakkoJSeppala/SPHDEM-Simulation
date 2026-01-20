using System;
using System.Collections.Generic;
using System.Numerics;

namespace ShipDamperSim.Core
{
    public class EnergyTracker
    {
        public float ShipKinetic { get; private set; }
        public float GranularKinetic { get; private set; }
        public float Dissipated { get; private set; }

        private float lastShipKinetic;
        private float lastGranularKinetic;

        public void Update(float shipAngularVel, float shipInertia, List<Particle> particles)
        {
            ShipKinetic = 0.5f * shipInertia * shipAngularVel * shipAngularVel;
            GranularKinetic = 0f;
            foreach (var p in particles)
            {
                GranularKinetic += 0.5f * p.Mass * p.Velocity.LengthSquared();
            }
            // Dissipaatio: erotus edelliseen (yksinkertaistettu, tarkenna tarvittaessa)
            float totalKinetic = ShipKinetic + GranularKinetic;
            float lastTotal = lastShipKinetic + lastGranularKinetic;
            if (lastTotal > 0 && totalKinetic < lastTotal)
                Dissipated += lastTotal - totalKinetic;
            lastShipKinetic = ShipKinetic;
            lastGranularKinetic = GranularKinetic;
        }
    }
}
