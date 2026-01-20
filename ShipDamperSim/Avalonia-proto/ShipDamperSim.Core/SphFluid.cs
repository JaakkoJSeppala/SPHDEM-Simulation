using OpenTK.Mathematics;
using System;
using System.Collections.Generic;


namespace ShipDamperSim.Core;

/// <summary>
/// Yksinkertainen 3D SPH-nestemalli laivatankille. Luo partikkelit ja boundaryt annetun parametrirakenteen pohjalta.
/// Fysikaaliset oletukset: ei pintajännitystä, perusviskositeetti, kiinteät seinät.
/// </summary>
public class SphFluid
{
    private readonly SimulationParameters _parameters;
    /// <summary>
    /// Yksittäinen neste- tai boundary-partikkeli (3D).
    /// </summary>
    public class Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Mass;
        public float Density;
        public float Pressure;
    }

    /// <summary>
    /// Rajapartikkeli (seinä, pohja, katto).
    /// </summary>
    public class BoundaryParticle : Particle { }
    public List<BoundaryParticle> Boundaries { get; } = new List<BoundaryParticle>();
    public float CFL = 0.25f; // CFL-kerroin

    public List<Particle> Particles { get; } = new List<Particle>();

    // SPH-parametrit
    public float SmoothingLength = 0.13f;
    public float RestDensity = 1000f;
    public float Gamma = 7.0f; // Taitin EOS:n gamma
    public float C0 = 20.0f;   // Taitin EOS:n c0 (äänennopeus)
    public float Mu = 3.5f;    // viskositeetti

    /// <summary>
    /// Luo SPH-nesteen ja boundaryt annetuilla parametreilla.
    /// </summary>
    public SphFluid(SimulationParameters parameters)
    {
        _parameters = parameters;
        var tank = parameters.Tank;
        var sph = parameters.Sph;
        float tankL = (float)tank.Length, tankW = (float)tank.Width, tankH = (float)tank.Height;
        float fillH = (float)tank.FillLevel;
        float minX = -tankL / 2, maxX = tankL / 2;
        float minY = 0.0f, maxY = fillH;
        float minZ = -tankW / 2, maxZ = tankW / 2;
        float wallSpacing = (float)sph.Resolution;
        float fluidSpacing = (float)sph.Resolution;

        // Pohja
        for (float x = minX; x <= maxX + 1e-6; x += wallSpacing)
            for (float z = minZ; z <= maxZ + 1e-6; z += wallSpacing)
                Boundaries.Add(new BoundaryParticle
                {
                    Position = new Vector3(x, minY, z),
                    Velocity = Vector3.Zero,
                    Mass = 0.02f,
                    Density = RestDensity,
                    Pressure = 0f
                });
        // Katto
        for (float x = minX; x <= maxX + 1e-6; x += wallSpacing)
            for (float z = minZ; z <= maxZ + 1e-6; z += wallSpacing)
                Boundaries.Add(new BoundaryParticle
                {
                    Position = new Vector3(x, tankH, z),
                    Velocity = Vector3.Zero,
                    Mass = 0.02f,
                    Density = RestDensity,
                    Pressure = 0f
                });
        // Seinät (vasen/oikea)
        for (float y = minY; y <= tankH + 1e-6; y += wallSpacing)
            for (float z = minZ; z <= maxZ + 1e-6; z += wallSpacing)
            {
                Boundaries.Add(new BoundaryParticle { Position = new Vector3(minX, y, z), Velocity = Vector3.Zero, Mass = 0.02f, Density = RestDensity, Pressure = 0f });
                Boundaries.Add(new BoundaryParticle { Position = new Vector3(maxX, y, z), Velocity = Vector3.Zero, Mass = 0.02f, Density = RestDensity, Pressure = 0f });
            }
        // Seinät (etu/taka)
        for (float x = minX; x <= maxX + 1e-6; x += wallSpacing)
            for (float y = minY; y <= tankH + 1e-6; y += wallSpacing)
            {
                Boundaries.Add(new BoundaryParticle { Position = new Vector3(x, y, minZ), Velocity = Vector3.Zero, Mass = 0.02f, Density = RestDensity, Pressure = 0f });
                Boundaries.Add(new BoundaryParticle { Position = new Vector3(x, y, maxZ), Velocity = Vector3.Zero, Mass = 0.02f, Density = RestDensity, Pressure = 0f });
            }
        // Neste-partikkelit (täyttö vain h <= fillH)
        for (float x = minX + fluidSpacing / 2; x <= maxX - fluidSpacing / 2 + 1e-6; x += fluidSpacing)
            for (float y = minY + fluidSpacing / 2; y <= maxY - fluidSpacing / 2 + 1e-6; y += fluidSpacing)
                for (float z = minZ + fluidSpacing / 2; z <= maxZ - fluidSpacing / 2 + 1e-6; z += fluidSpacing)
                {
                    Particles.Add(new Particle
                    {
                        Position = new Vector3(x, y, z),
                        Velocity = Vector3.Zero,
                        Mass = 0.02f,
                        Density = RestDensity,
                        Pressure = 0f
                    });
                }
    }

    public void Step(float dt)
    {
        // TODO: SPH-fysiikka (täydennä tarvittaessa)
    }
}
