using OpenTK.Mathematics;
using System;
using System.Collections.Generic;


namespace ShipDamperSim.Core;

/// <summary>
/// Rajapinta damper-tyypeille. Mahdollistaa useiden vaimenninratkaisujen käytön.
/// </summary>
public interface IDamper
{
    List<DemDamper.Particle> Particles { get; }
    void Step(float dt);
}

/// <summary>
/// Yksinkertainen 3D DEM-damperi. Luo rakeet ja rajat annetun parametrirakenteen pohjalta.
/// Fysikaaliset oletukset: pallomaiset rakeet, kimmoisa törmäys, yksinkertainen kitka.
/// </summary>
public class DemDamper : IDamper
{
    private readonly SimulationParameters _parameters;
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// Yksittäinen DEM-rae (3D).
    /// </summary>
    public class Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Mass;
        public float Radius;
    }

    public List<Particle> Particles { get; } = new List<Particle>();
    public float Restitution = 0.5f;
    public float Friction = 0.2f;
    public Vector3 Gravity = new Vector3(0, -9.81f, 0);

    /// <summary>
    /// Luo damperin rakeet ja rajat annetuilla parametreilla.
    /// </summary>
    public DemDamper(SimulationParameters parameters)
    {
        _parameters = parameters;
        var damper = parameters.Damper;
        float Ld = (float)damper.Length, Wd = (float)damper.Width, Hd = (float)damper.Height;
        float dp = (float)damper.ParticleDiameter;
        float damperX = (float)damper.Position.X;
        float damperY = (float)damper.Position.Y;
        float damperZ = (float)damper.Position.Z;
        float eta = (float)damper.FillFraction;
        float density = (float)damper.Density;
        float damperVol = Ld * Wd * Hd;
        float particleVol = (4f / 3f) * (float)Math.PI * (float)Math.Pow(dp / 2, 3);
        int nParticles = (int)(eta * damperVol / particleVol);

        var rng = new Random();
        for (int i = 0; i < nParticles; i++)
        {
            float x = damperX + (float)(rng.NextDouble() * (Ld - dp) - (Ld / 2 - dp / 2));
            float y = damperY + (float)(rng.NextDouble() * (Hd - dp) - (Hd / 2 - dp / 2));
            float z = damperZ + (float)(rng.NextDouble() * (Wd - dp) - (Wd / 2 - dp / 2));
            Particles.Add(new Particle
            {
                Position = new Vector3(x, y, z),
                Velocity = Vector3.Zero,
                Mass = density * particleVol,
                Radius = dp / 2
            });
        }
    }

    public void Step(float dt)
    {
        var damper = _parameters.Damper;
        float Ld = (float)damper.Length, Wd = (float)damper.Width, Hd = (float)damper.Height;
        float damperX = (float)damper.Position.X;
        float damperY = (float)damper.Position.Y;
        float damperZ = (float)damper.Position.Z;
        float boxX0 = damperX - Ld / 2, boxX1 = damperX + Ld / 2;
        float boxY0 = damperY - Hd / 2, boxY1 = damperY + Hd / 2;
        float boxZ0 = damperZ - Wd / 2, boxZ1 = damperZ + Wd / 2;
        foreach (var p in Particles)
        {
            p.Velocity += Gravity * dt;
            p.Position += p.Velocity * dt;
            if (p.Position.X < boxX0) { p.Position.X = boxX0; p.Velocity.X *= -Restitution; }
            if (p.Position.X > boxX1) { p.Position.X = boxX1; p.Velocity.X *= -Restitution; }
            if (p.Position.Y < boxY0) { p.Position.Y = boxY0; p.Velocity.Y *= -Restitution; }
            if (p.Position.Y > boxY1) { p.Position.Y = boxY1; p.Velocity.Y *= -Restitution; }
            if (p.Position.Z < boxZ0) { p.Position.Z = boxZ0; p.Velocity.Z *= -Restitution; }
            if (p.Position.Z > boxZ1) { p.Position.Z = boxZ1; p.Velocity.Z *= -Restitution; }
        }
        // DEM-partikkelien väliset törmäykset (yksinkertainen, ei kitkaa)
    }
}
