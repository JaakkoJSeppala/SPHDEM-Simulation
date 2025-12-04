namespace ResonanceSimulation.Core;

/// <summary>
/// SPH-solver joka laskee tiheyden, paineen ja voimat kaikille nesteen partikkeleille.
/// Käyttää WCSPH (Weakly Compressible SPH) -mallia.
/// </summary>
public class SPHSolver
{
    private readonly SimulationConfig _config;

    public SPHSolver(SimulationConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Laske tiheys kaikille partikkeleille.
    /// ρᵢ = Σⱼ mⱼ W(rᵢⱼ, h)
    /// </summary>
    public void ComputeDensity(List<SPHParticle> particles)
    {
        foreach (var pi in particles)
        {
            double density = 0.0;

            foreach (var pj in particles)
            {
                Vector2D rij = pi.Position - pj.Position;
                double r = rij.Length();

                if (r < 2.0 * _config.SmoothingLength)
                {
                    density += pj.Mass * WendlandKernel.W(r, _config.SmoothingLength);
                }
            }

            pi.Density = Math.Max(density, _config.RestDensity); // Estä liian pienet tiheydet
        }
    }

    /// <summary>
    /// Laske paine Tait-tilanyhtälöllä.
    /// p = B[(ρ/ρ₀)^γ - 1]
    /// </summary>
    public void ComputePressure(List<SPHParticle> particles)
    {
        const double gamma = 7.0; // Vesi
        double B = _config.Stiffness;

        foreach (var p in particles)
        {
            double ratio = p.Density / _config.RestDensity;
            p.Pressure = B * (Math.Pow(ratio, gamma) - 1.0);
        }
    }

    /// <summary>
    /// Laske painevoima.
    /// Fᵢ = -mᵢ Σⱼ mⱼ (pᵢ/ρᵢ² + pⱼ/ρⱼ²) ∇W(rᵢⱼ, h)
    /// </summary>
    public void ComputePressureForce(List<SPHParticle> particles)
    {
        foreach (var pi in particles)
        {
            Vector2D force = Vector2D.Zero;

            foreach (var pj in particles)
            {
                if (pi == pj) continue;

                Vector2D rij = pi.Position - pj.Position;
                double r = rij.Length();

                if (r < 2.0 * _config.SmoothingLength && r > 1e-12)
                {
                    double gradW = WendlandKernel.GradW(r, _config.SmoothingLength);
                    Vector2D direction = rij / r;

                    double pressureTerm = pi.Pressure / (pi.Density * pi.Density)
                                        + pj.Pressure / (pj.Density * pj.Density);

                    force += -pj.Mass * pressureTerm * gradW * direction;
                }
            }

            pi.PressureForce = pi.Mass * force;
        }
    }

    /// <summary>
    /// Laske viskositeetti (Morris et al. 1997).
    /// Fᵢ = μ mᵢ Σⱼ mⱼ (vⱼ - vᵢ) / ρⱼ ∇²W(rᵢⱼ, h)
    /// </summary>
    public void ComputeViscosity(List<SPHParticle> particles)
    {
        foreach (var pi in particles)
        {
            Vector2D force = Vector2D.Zero;

            foreach (var pj in particles)
            {
                if (pi == pj) continue;

                Vector2D rij = pi.Position - pj.Position;
                double r = rij.Length();

                if (r < 2.0 * _config.SmoothingLength)
                {
                    double laplacianW = WendlandKernel.LaplacianW(r, _config.SmoothingLength);
                    Vector2D velocityDiff = pj.Velocity - pi.Velocity;

                    force += pj.Mass * velocityDiff / pj.Density * laplacianW;
                }
            }

            pi.ViscosityForce = _config.Viscosity * pi.Mass * force;
        }
    }

    /// <summary>
    /// Täydellinen SPH-iteraatio: tiheys → paine → voimat.
    /// </summary>
    public void Step(List<SPHParticle> particles)
    {
        // 1. Laske tiheys
        ComputeDensity(particles);

        // 2. Laske paine
        ComputePressure(particles);

        // 3. Laske voimat
        ComputePressureForce(particles);
        ComputeViscosity(particles);
    }
}
