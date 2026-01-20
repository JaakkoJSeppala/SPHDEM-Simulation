using System;
using System.Collections.Generic;
using System.Numerics;

namespace ShipDamperSim.Core
{
    /// <summary>
    /// Represents a single granular particle.
    /// </summary>
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Radius;
        public float Mass;
    }

    /// <summary>
    /// Discrete Element Method (DEM) engine for a granular damper.
    /// 2D implementation: linear normal spring + viscous damping + Coulomb friction.
    /// </summary>
    public class DemEngine
    {
        // -------------------------------
        // Public configuration
        // -------------------------------

        public List<Particle> Particles { get; } = new();

        /// <summary>Normal contact stiffness (N/m).</summary>
        public float Kn { get; set; } = 1.0e4f;

        /// <summary>Normal viscous damping (Ns/m).</summary>
        public float GammaN { get; set; } = 20f;

        /// <summary>Coulomb friction coefficient.</summary>
        public float Mu { get; set; } = 0.3f;

        /// <summary>Gravity acting in -Y direction.</summary>
        public Vector2 Gravity { get; set; } = new(0f, -9.81f);

        // -------------------------------
        // Public API
        // -------------------------------

        /// <summary>
        /// Advances the DEM simulation by one time step.
        /// </summary>
        public void Step(float dt)
        {
            int n = Particles.Count;

            // 1) Apply gravity
            for (int i = 0; i < n; i++)
            {
                var p = Particles[i];
                p.Velocity += Gravity * dt;
            }

            // 2) Particle–particle contacts
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    ResolveContact(Particles[i], Particles[j], dt);
                }
            }

            // 3) Integrate positions (explicit Euler)
            for (int i = 0; i < n; i++)
            {
                var p = Particles[i];
                p.Position += p.Velocity * dt;
            }
        }

        /// <summary>
        /// Returns particle positions for 3D visualization (Z = 0).
        /// </summary>
        public IEnumerable<(float X, float Y, float Z)> ParticlesForVisualization()
        {
            foreach (var p in Particles)
                yield return (p.Position.X, p.Position.Y, 0f);
        }

        // -------------------------------
        // Internal DEM physics
        // -------------------------------

        private void ResolveContact(Particle pi, Particle pj, float dt)
        {
            Vector2 d = pj.Position - pi.Position;
            float dist2 = d.LengthSquared();
            float rSum = pi.Radius + pj.Radius;

            if (dist2 >= rSum * rSum)
                return;

            float dist = MathF.Sqrt(MathF.Max(dist2, 1e-12f));
            Vector2 n = d / dist;
            float overlap = rSum - dist;

            // Relative velocity
            Vector2 relVel = pj.Velocity - pi.Velocity;
            float vN = Vector2.Dot(relVel, n);

            // Normal force: spring + damping
            float fn = Kn * overlap - GammaN * vN;
            if (fn < 0f) fn = 0f;

            Vector2 fN = fn * n;

            // Tangential friction (Coulomb)
            Vector2 vT = relVel - vN * n;
            float vTmag = vT.Length();

            Vector2 fT = Vector2.Zero;
            if (vTmag > 1e-6f)
            {
                Vector2 t = vT / vTmag;
                float ftMax = Mu * fn;
                fT = -ftMax * t;
            }

            Vector2 f = fN + fT;

            // Newton's third law
            pi.Velocity -= (f / pi.Mass) * dt;
            pj.Velocity += (f / pj.Mass) * dt;
        }
    }
}
