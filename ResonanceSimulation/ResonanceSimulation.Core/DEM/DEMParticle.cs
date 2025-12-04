namespace ResonanceSimulation.Core;

/// <summary>
/// DEM-partikkeli joka edustaa kiinteää palloa (granulaaridamperi).
/// Käyttää Hertz-Mindlin kontaktimallia.
/// </summary>
public class DEMParticle
{
    // Kinemaattiset muuttujat
    public Vector2D Position { get; set; }
    public Vector2D Velocity { get; set; }
    public Vector2D Acceleration { get; set; }
    public double AngularVelocity { get; set; }
    public double AngularAcceleration { get; set; }

    // Fysikaaliset ominaisuudet
    public double Radius { get; init; }
    public double Mass { get; init; }
    public double Inertia { get; init; }

    // Materiaaliparametrit
    public double YoungsModulus { get; init; }      // E [Pa]
    public double PoissonRatio { get; init; }       // ν [-]
    public double RestitutionCoeff { get; init; }   // e [-]
    public double FrictionCoeff { get; init; }      // μ [-]

    // Väliaikaiset laskenta-arvot
    public Vector2D ContactForce { get; set; }
    public Vector2D FluidForce { get; set; }
    public double ContactTorque { get; set; }

    public DEMParticle(Vector2D position, double radius, double density,
                       double youngsModulus = 1e7, double poissonRatio = 0.3,
                       double restitutionCoeff = 0.5, double frictionCoeff = 0.3)
    {
        Position = position;
        Velocity = Vector2D.Zero;
        Acceleration = Vector2D.Zero;
        AngularVelocity = 0.0;
        AngularAcceleration = 0.0;

        Radius = radius;
        double volume = Math.PI * radius * radius; // 2D: pinta-ala per yksikkösyvyys
        Mass = volume * density;
        Inertia = 0.5 * Mass * radius * radius; // Levyn hitausmomentti

        YoungsModulus = youngsModulus;
        PoissonRatio = poissonRatio;
        RestitutionCoeff = restitutionCoeff;
        FrictionCoeff = frictionCoeff;

        ContactForce = Vector2D.Zero;
        FluidForce = Vector2D.Zero;
        ContactTorque = 0.0;
    }

    /// <summary>
    /// Nollaa laskennalliset voimat ja momentit.
    /// </summary>
    public void ResetForces()
    {
        ContactForce = Vector2D.Zero;
        FluidForce = Vector2D.Zero;
        ContactTorque = 0.0;
        Acceleration = Vector2D.Zero;
        AngularAcceleration = 0.0;
    }

    /// <summary>
    /// Päivitä kiihtyvyydet voimien perusteella.
    /// </summary>
    public void UpdateAcceleration(Vector2D gravity)
    {
        Vector2D totalForce = ContactForce + FluidForce + Mass * gravity;
        Acceleration = totalForce / Mass;
        AngularAcceleration = ContactTorque / Inertia;
    }
}
