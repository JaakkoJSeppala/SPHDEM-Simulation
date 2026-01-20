using System.Text.Json;
using System.Globalization;
/// <summary>
/// Simulaation tulosrakenne, sisältää tallennusmetodit CSV- ja JSON-muotoon.
/// </summary>
public class SimulationResult
{
    public double MaxRollOn { get; }
    public double MaxRollOff { get; }
    public double BenefitPercent { get; }
    public List<(double t, double roll)> RollDataOn { get; }
    public List<(double t, double roll)> RollDataOff { get; }

    public SimulationResult(double maxRollOn, double maxRollOff, double benefit, List<(double t, double roll)> on, List<(double t, double roll)> off)
    {
        MaxRollOn = maxRollOn;
        MaxRollOff = maxRollOff;
        BenefitPercent = benefit;
        RollDataOn = on;
        RollDataOff = off;
    }

    public void SaveCsv(string path)
    {
        using var sw = new StreamWriter(path);
        sw.WriteLine("t,roll_on,roll_off");
        int n = Math.Max(RollDataOn.Count, RollDataOff.Count);
        for (int i = 0; i < n; i++)
        {
            string t = i < RollDataOn.Count ? RollDataOn[i].t.ToString(CultureInfo.InvariantCulture) : "";
            string rollOn = i < RollDataOn.Count ? RollDataOn[i].roll.ToString(CultureInfo.InvariantCulture) : "";
            string rollOff = i < RollDataOff.Count ? RollDataOff[i].roll.ToString(CultureInfo.InvariantCulture) : "";
            sw.WriteLine($"{t},{rollOn},{rollOff}");
        }
    }

    public void SaveJson(string path)
    {
        var obj = new
        {
            MaxRollOn,
            MaxRollOff,
            BenefitPercent,
            RollDataOn,
            RollDataOff
        };
        File.WriteAllText(path, JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
    }
}

using System;
using System.Collections.Generic;
namespace ShipDamperSim.Core;

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
            foreach (var p in simOn.Dem.Particles) { demMass += p.Mass; demXsum += p.Mass * p.Position.X; }
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
            foreach (var p in simOff.Dem.Particles) { demMass += p.Mass; demXsum += p.Mass * p.Position.X; }
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
