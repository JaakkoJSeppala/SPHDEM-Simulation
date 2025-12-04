namespace ResonanceSimulation.Core;

/// <summary>
/// SPH-partikkeli joka edustaa nestettä (vesi).
/// Käyttää Weakly Compressible SPH (WCSPH) -mallia.
/// </summary>
public class SPHParticle
{
    // Kinemaattiset muuttujat
    public Vector2D Position { get; set; }
    public Vector2D Velocity { get; set; }
    public Vector2D Acceleration { get; set; }

    // Fysikaaliset ominaisuudet
    public double Mass { get; init; }
    public double Density { get; set; }
    public double Pressure { get; set; }

    // SPH-parametrit
    public double SmoothingLength { get; init; }

    // Väliaikaiset laskenta-arvot
    public Vector2D PressureForce { get; set; }
    public Vector2D ViscosityForce { get; set; }
    public Vector2D ExternalForce { get; set; }

    public SPHParticle(Vector2D position, double mass, double smoothingLength)
    {
        Position = position;
        Velocity = Vector2D.Zero;
        Acceleration = Vector2D.Zero;
        Mass = mass;
        SmoothingLength = smoothingLength;
        Density = 1000.0; // Veden tiheys kg/m³
        Pressure = 0.0;
        PressureForce = Vector2D.Zero;
        ViscosityForce = Vector2D.Zero;
        ExternalForce = Vector2D.Zero;
    }

    /// <summary>
    /// Nollaa laskennalliset voimat ennen seuraavaa iteraatiota.
    /// </summary>
    public void ResetForces()
    {
        PressureForce = Vector2D.Zero;
        ViscosityForce = Vector2D.Zero;
        ExternalForce = Vector2D.Zero;
        Acceleration = Vector2D.Zero;
    }

    /// <summary>
    /// Laske kokonaisvoima ja kiihtyvyys.
    /// </summary>
    public void UpdateAcceleration(Vector2D gravity)
    {
        Vector2D totalForce = PressureForce + ViscosityForce + ExternalForce + Mass * gravity;
        Acceleration = totalForce / Mass;
    }
}
