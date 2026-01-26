using System.Text.Json;
using System.Globalization;
using System;
using System.Collections.Generic;

namespace ShipDamperSim.Core
{


    /// <summary>
    /// Korkean tason simulaatioajuri: ajaa simulaation, kerää tulokset ja KPI:t.
    /// Soveltuu batch-ajoihin ja testaukseen.
    /// </summary>
    public static class Simulator
    {
        /// <summary>
        /// Ajaa simulaation annetulla parametrirakenteella ja palauttaa tulokset.
        /// </summary>
        public static SimulationResult Run(SimulationParameters parameters)
        {
            // Simulaatioaika ja askelpituus parametreista
            double simTime = 10.0, dt = 0.002;
            if (parameters != null && parameters.Tank != null)
            {
                // Voit laajentaa parametreja SimulationParametersiin tarpeen mukaan
                // simTime ja dt voidaan lukea myös parametreista, jos lisäät ne jsoniin
            }
            int steps = (int)(simTime / dt);

            // Simuloi vaimentimella
            var simOn = new Simulation(parameters) { DamperEnabled = true };
            var rollDataOn = new List<(double t, double roll)>();
            for (int i = 0; i < steps; i++)
            {
                double t = i * dt;
                simOn.Step(dt);
                double demMass = 0, demXsum = 0;
                foreach (var p in simOn.Damper.Particles) { demMass += p.Mass; demXsum += p.Mass * p.Position.X; }
                double demXc = demMass > 0 ? demXsum / demMass : 0;
                rollDataOn.Add((t, demXc));
            }
            double maxRollOn = 0;
            foreach (var (_, roll) in rollDataOn)
                if (Math.Abs(roll) > maxRollOn) maxRollOn = Math.Abs(roll);

            // Simuloi ilman vaimenninta
            var simOff = new Simulation(parameters) { DamperEnabled = false };
            var rollDataOff = new List<(double t, double roll)>();
            for (int i = 0; i < steps; i++)
            {
                double t = i * dt;
                simOff.Step(dt);
                double demMass = 0, demXsum = 0;
                foreach (var p in simOff.Damper.Particles) { demMass += p.Mass; demXsum += p.Mass * p.Position.X; }
                double demXc = demMass > 0 ? demXsum / demMass : 0;
                rollDataOff.Add((t, demXc));
            }
            double maxRollOff = 0;
            foreach (var (_, roll) in rollDataOff)
                if (Math.Abs(roll) > maxRollOff) maxRollOff = Math.Abs(roll);

            // Laske hyötyprosentti
            double benefit = (maxRollOff > 0) ? 100.0 * (maxRollOff - maxRollOn) / maxRollOff : 0.0;

            return new SimulationResult(
                maxRollOn,
                maxRollOff,
                benefit,
                rollDataOn,
                rollDataOff
            );
        }
    }
}
