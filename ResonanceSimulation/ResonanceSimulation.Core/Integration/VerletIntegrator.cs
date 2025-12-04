namespace ResonanceSimulation.Core;

/// <summary>
/// Verlet-integraattori aikaintegrointiin.
/// Käyttää Velocity Verlet -algoritmia (symplektinen, 2. kertaluvun tarkkuus).
/// </summary>
public class VerletIntegrator
{
    private readonly double _dt;

    public VerletIntegrator(double timeStep)
    {
        _dt = timeStep;
    }

    /// <summary>
    /// Integroi SPH-partikkelit yhdellä aikaaskeleella.
    /// </summary>
    public void Integrate(List<SPHParticle> particles, Vector2D gravity)
    {
        foreach (var p in particles)
        {
            // Päivitä kiihtyvyys voimien perusteella
            p.UpdateAcceleration(gravity);

            // Velocity Verlet: v(t + dt/2) = v(t) + a(t) dt/2
            Vector2D halfStepVelocity = p.Velocity + p.Acceleration * (_dt / 2.0);

            // Päivitä paikka: x(t + dt) = x(t) + v(t + dt/2) dt
            p.Position += halfStepVelocity * _dt;

            // Päivitä nopeus: v(t + dt) = v(t + dt/2) + a(t + dt) dt/2
            // (huom: a(t + dt) lasketaan seuraavalla kierroksella)
            p.Velocity = halfStepVelocity + p.Acceleration * (_dt / 2.0);
        }
    }

    /// <summary>
    /// Integroi DEM-partikkelit (translaatio + rotaatio).
    /// </summary>
    public void Integrate(List<DEMParticle> particles, Vector2D gravity)
    {
        foreach (var p in particles)
        {
            // Päivitä kiihtyvyydet
            p.UpdateAcceleration(gravity);

            // Translaatio (Velocity Verlet)
            Vector2D halfStepVelocity = p.Velocity + p.Acceleration * (_dt / 2.0);
            p.Position += halfStepVelocity * _dt;
            p.Velocity = halfStepVelocity + p.Acceleration * (_dt / 2.0);

            // Rotaatio (vastaava algoritmi kulmanopeudelle)
            double halfStepAngularVelocity = p.AngularVelocity + p.AngularAcceleration * (_dt / 2.0);
            p.AngularVelocity = halfStepAngularVelocity + p.AngularAcceleration * (_dt / 2.0);
        }
    }
}
