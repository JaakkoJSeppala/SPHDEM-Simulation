using System.Collections.Generic;
using System.Numerics;

namespace ShipDamperSim.Core
{
    public class DamperSimulator
    {
        public DemEngine Dem { get; } = new();
        public RollSimulator Roll { get; } = new();
        public EnergyTracker Energy { get; } = new();

        public float DamperArm = 2.0f; // etäisyys laivan keskiöstä
        public float Time { get; private set; }
        public List<float> TimeHistory { get; } = new();
        public List<float> RollHistory { get; } = new();
        public List<float> ShipKinHistory { get; } = new();
        public List<float> GranKinHistory { get; } = new();
        public List<float> DissHistory { get; } = new();

        public void Step(float dt)
        {
            // Päivitä rae–rae DEM
            Dem.Step(dt);
            // Laske damperin momentti (yksinkertaistettu: kaikkien rakeiden voima x varsi)
            float damperMoment = 0f;
            foreach (var p in Dem.Particles)
            {
                damperMoment += p.Mass * p.Position.X * DamperArm; // Oletus: X-akseli kohtisuorassa rullaan
            }
            // Päivitä rullaus
            Roll.Step(dt, damperMoment);
            // Päivitä energiat
            Energy.Update(Roll.AngularVelocity, Roll.Inertia, Dem.Particles);
            // Tallenna historia
            Time += dt;
            TimeHistory.Add(Time);
            RollHistory.Add(Roll.Angle);
            ShipKinHistory.Add(Energy.ShipKinetic);
            GranKinHistory.Add(Energy.GranularKinetic);
            DissHistory.Add(Energy.Dissipated);
        }
    }
}
