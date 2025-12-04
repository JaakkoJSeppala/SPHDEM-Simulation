namespace ResonanceSimulation.Core;

/// <summary>
/// DEM-solver joka laskee kontaktivoimat Hertz-Mindlin-mallilla.
/// Toteuttaa normaalin ja tangentiaalisen kontaktin.
/// </summary>
public class DEMSolver
{
    private readonly SimulationConfig _config;

    public DEMSolver(SimulationConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Laske kontaktivoimat kaikille DEM-partikkeleille (pallo-pallo).
    /// Käyttää Hertz-Mindlin kontaktimallia.
    /// </summary>
    public void ComputeContacts(List<DEMParticle> particles)
    {
        for (int i = 0; i < particles.Count; i++)
        {
            for (int j = i + 1; j < particles.Count; j++)
            {
                var pi = particles[i];
                var pj = particles[j];

                Vector2D rij = pj.Position - pi.Position;
                double distance = rij.Length();
                double overlap = (pi.Radius + pj.Radius) - distance;

                if (overlap > 0)
                {
                    // Kontakti on olemassa
                    ProcessContact(pi, pj, rij, distance, overlap);
                }
            }
        }
    }

    /// <summary>
    /// Käsittele yksittäinen kontakti Hertz-Mindlin-mallilla.
    /// </summary>
    private void ProcessContact(DEMParticle pi, DEMParticle pj, Vector2D rij, double distance, double overlap)
    {
        // Kontaktin normaalivektori
        Vector2D normal = rij / distance;

        // Efektiiviset parametrit
        double Reff = (pi.Radius * pj.Radius) / (pi.Radius + pj.Radius);
        double Meff = (pi.Mass * pj.Mass) / (pi.Mass + pj.Mass);
        double Eeff = 1.0 / ((1.0 - pi.PoissonRatio * pi.PoissonRatio) / pi.YoungsModulus +
                              (1.0 - pj.PoissonRatio * pj.PoissonRatio) / pj.YoungsModulus);

        // Normaalijäykkyys (Hertz)
        double kn = 4.0 / 3.0 * Eeff * Math.Sqrt(Reff);
        double normalStiffness = kn * Math.Pow(overlap, 0.5);

        // Normaalivaimennus (kriittinen vaimennus e:n perusteella)
        double e = (pi.RestitutionCoeff + pj.RestitutionCoeff) / 2.0;
        double cn = -2.0 * Math.Sqrt(5.0 / 6.0) * Math.Log(e) / Math.Sqrt(Math.Log(e) * Math.Log(e) + Math.PI * Math.PI);
        cn *= 2.0 * Math.Sqrt(Meff * normalStiffness);

        // Suhteellinen nopeus
        Vector2D relativeVelocity = pj.Velocity - pi.Velocity;
        double normalVelocity = Vector2D.Dot(relativeVelocity, normal);

        // Normaalikontaktivoima
        double Fn = normalStiffness * overlap + cn * normalVelocity;
        Vector2D normalForce = Fn * normal;

        // Tangentiaalisuunta
        Vector2D tangent = relativeVelocity - normalVelocity * normal;
        double tangentSpeed = tangent.Length();

        Vector2D frictionForce = Vector2D.Zero;
        if (tangentSpeed > 1e-12)
        {
            Vector2D tangentDirection = tangent / tangentSpeed;

            // Coulombin kitka
            double mu = (pi.FrictionCoeff + pj.FrictionCoeff) / 2.0;
            double maxFriction = mu * Math.Abs(Fn);

            // Tangentiaalijäykkyys
            double kt = normalStiffness * 0.5; // Yksinkertaistettu malli
            double Ft = Math.Min(kt * tangentSpeed * _config.TimeStep, maxFriction);

            frictionForce = -Ft * tangentDirection;
        }

        // Lisää voimat partikkeleille (Newtonin 3. laki)
        Vector2D totalForce = normalForce + frictionForce;
        pi.ContactForce += -totalForce;
        pj.ContactForce += totalForce;

        // Momentti kitkasta (pyöriminen)
        double torqueMagnitude = frictionForce.Length() * pi.Radius;
        pi.ContactTorque += torqueMagnitude;
        pj.ContactTorque += -torqueMagnitude;
    }

    /// <summary>
    /// Laske kontaktivoimat seinien kanssa (yksinkertaistettu).
    /// </summary>
    public void ComputeWallContacts(List<DEMParticle> particles, TankGeometry tank)
    {
        foreach (var p in particles)
        {
            // Vasen seinä
            double overlapLeft = p.Radius - (p.Position.X - tank.MinX);
            if (overlapLeft > 0)
            {
                ApplyWallForce(p, Vector2D.UnitX, overlapLeft);
            }

            // Oikea seinä
            double overlapRight = p.Radius - (tank.MaxX - p.Position.X);
            if (overlapRight > 0)
            {
                ApplyWallForce(p, -Vector2D.UnitX, overlapRight);
            }

            // Pohja
            double overlapBottom = p.Radius - (p.Position.Y - tank.MinY);
            if (overlapBottom > 0)
            {
                ApplyWallForce(p, Vector2D.UnitY, overlapBottom);
            }
        }
    }

    private void ApplyWallForce(DEMParticle p, Vector2D normal, double overlap)
    {
        // Yksinkertaistettu seinäkontakti (lineaarinen jousi + vaimennus)
        double kn = 1e5; // Jäykkyys
        double cn = 100.0; // Vaimennus

        double normalVelocity = Vector2D.Dot(p.Velocity, normal);
        double Fn = kn * overlap - cn * normalVelocity;

        p.ContactForce += Fn * normal;
    }
}
