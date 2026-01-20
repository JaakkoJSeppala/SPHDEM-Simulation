/// <summary>
/// Rajapinta mittarityypeille. Mahdollistaa uusien mittareiden lisäämisen helposti.
/// </summary>
public interface IMeter
{
    void Measure(Simulation sim, double time);
    string Name { get; }
    string GetCsvHeader();
    string GetCsvLine();
}

/// <summary>
/// Esimerkkimittari: kokonaisenergian seuranta.
/// </summary>
public class EnergyMeter : IMeter
{
    private double _lastEnergy;
    public string Name => "TotalEnergy";
    public void Measure(Simulation sim, double time)
    {
        // Esimerkki: lasketaan nesteen ja damperin kineettinen energia
        double ekin = 0;
        foreach (var p in sim.Sph.Particles)
            ekin += 0.5 * p.Mass * p.Velocity.LengthSquared;
        foreach (var p in sim.Damper.Particles)
            ekin += 0.5 * p.Mass * p.Velocity.LengthSquared;
        _lastEnergy = ekin;
    }
    public string GetCsvHeader() => Name;
    public string GetCsvLine() => _lastEnergy.ToString("G17");
}
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace ShipDamperSim.Core;

/// <summary>
/// Kokoaa SPH- ja damper-mallit yhteen simulaatioksi. Hoitaa askeleen ja kytkennän.
/// Fysikaaliset oletukset: tankki jäykkä, damperi liikkuu tankin mukana, kytkentä SPH-DEM.
///
/// Laajennettavuus: Toteuta uusi damperi (IDamper) tai mittari (IMeter) ja anna ne Simulationille.
/// Rakenne: Simulaatiologiikka (core) on täysin erillään käyttöliittymästä ja visualisoinnista.
/// </summary>
public class Simulation
{
    public SphFluid Sph { get; }
    public IDamper Damper { get; }
    public double Time { get; private set; } = 0;
    public bool DamperEnabled { get; set; } = true;
    public SimulationParameters Parameters { get; }

    /// <summary>
    /// Luo simulaation annetuilla parametreilla (SPH, DEM, kytkentä).
    /// </summary>
    public Simulation(SimulationParameters parameters)
    {
        Parameters = parameters;
        Sph = new SphFluid(parameters);
        Damper = new DemDamper(parameters);
        // Alkuasetus: pieni kallistus
        double initialRoll = 0.1;
        foreach (var p in Sph.Particles)
            p.Position.X += (float)(Math.Sin(initialRoll) * (p.Position.Y - 0.05));
        foreach (var p in Damper.Particles)
            p.Position.X += (float)(Math.Sin(initialRoll) * (p.Position.Y - 0.05));
    }

    /// <summary>
    /// Suorittaa yhden simulaatioaskeleen (SPH, DEM, kytkentä).
    /// </summary>
    public void Step(double dt)
    {
        Sph.Step((float)dt);
        Damper.Step((float)dt);
        if (DamperEnabled)
            SphDemCoupler.Couple(Sph.Particles, Damper.Particles, (float)dt);
        Time += dt;
    }
}
