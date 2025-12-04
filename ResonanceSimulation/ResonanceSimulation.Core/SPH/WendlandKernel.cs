using System.Runtime.CompilerServices;

namespace ResonanceSimulation.Core;

/// <summary>
/// Wendland C2 kernel SPH:lle (compact support, C2-jatkuva).
/// Suositellaan gradun lähteissä (Dehnen & Aly 2012).
/// </summary>
public static class WendlandKernel
{
    private const double Alpha2D = 7.0 / (4.0 * Math.PI); // 2D normalisointikerroin

    /// <summary>
    /// Laske kernel-arvo W(r, h).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double W(double r, double h)
    {
        double q = r / h;
        if (q >= 2.0) return 0.0;

        double factor = 1.0 - 0.5 * q;
        return Alpha2D / (h * h) * factor * factor * factor * factor * (2.0 * q + 1.0);
    }

    /// <summary>
    /// Laske kernel-gradientin arvo ∇W(r, h).
    /// Palauttaa gradientti vektorina (skaalattuna yksikkövektorilla).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GradW(double r, double h)
    {
        if (r < 1e-12) return 0.0; // Vältä jako nollalla

        double q = r / h;
        if (q >= 2.0) return 0.0;

        double factor = 1.0 - 0.5 * q;
        return -Alpha2D / (h * h * h) * 5.0 * q * factor * factor * factor;
    }

    /// <summary>
    /// Laske kernel-Laplacian arvo ∇²W(r, h) (viskositeettia varten).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LaplacianW(double r, double h)
    {
        double q = r / h;
        if (q >= 2.0) return 0.0;

        double factor = 1.0 - 0.5 * q;
        double term1 = 5.0 * factor * factor * factor;
        double term2 = -7.5 * q * factor * factor;
        return Alpha2D / (h * h * h * h) * (term1 + term2);
    }
}
