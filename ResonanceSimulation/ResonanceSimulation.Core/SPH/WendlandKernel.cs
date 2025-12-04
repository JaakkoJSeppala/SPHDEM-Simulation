using System.Runtime.CompilerServices;

namespace ResonanceSimulation.Core;

/// <summary>
/// Wendland C2 kernel for SPH (compact support, C2 continuous).
/// Used for fluid particle interactions in WCSPH.
///
/// Normalization:
///   W(0, h) = 7 / (4π h²) for support q = r/h ∈ [0, 2].
///   This is derived by taking the normalization C = 7/π for support [0,1] (see Dehnen & Aly 2012, Table 1),
///   and dividing by 4 to account for the area scaling when the support is doubled to [0,2] (see also Price 2012, JCP 231, eq. 19).
///   The paper does not state this 1/4 factor explicitly, but it is standard in SPH literature and code.
///
/// References:
///   Dehnen, W., & Aly, H. (2012). Improving convergence in smoothed particle hydrodynamics simulations without pairing instability. MNRAS, 425(2), 1068–1082. doi:10.1111/j.1365-2966.2012.21573.x (see references.bib, entry: Dehnen2012)
///   Price, D. J. (2012). Smoothed particle hydrodynamics and magnetohydrodynamics. JCP, 231(3), 759–794. doi:10.1016/j.jcp.2010.12.011
/// 
/// ------------------------------------------------------------------------------
///Derivation of the normalization constant for the 2D Wendland C2 kernel
///with support q = r/h ∈ [0, 2].

///Kernel form:
///    W(r,h) = C * (1 - q/2)^4 * (2q + 1),   where q = r/h.

///We determine C by requiring that W integrates to 1 over R^2.

///Normalization condition in 2D:
///    ∫∫ W(r,h) dA = 1
///Using polar coordinates:
///    ∫(theta=0..2π) ∫(r=0..2h) W(r,h) * r dr dθ = 1

///Insert W and substitute q = r/h:
///    r = qh
///    dr = h dq
///    r dr = h^2 * q dq

///Thus:
///    1 = 2π * C * h^2 * ∫(q=0..2) q * (1 - q/2)^4 * (2q + 1) dq

///Define the integral:
///    I = ∫(0..2) q(1 - q/2)^4 (2q + 1) dq

///Expand the polynomial:
///    q(1 - q/2)^4 (2q + 1)
///      = (1/8) q^6 - (15/16) q^5 + (5/2) q^4 - (5/2) q^3 + q

///Integrate term-by-term over [0, 2]:
///    ∫ (1/8) q^6 dq       = 16/7
///    ∫ -(15/16) q^5 dq    = -10
///    ∫ (5/2) q^4 dq       = 16
///    ∫ -(5/2) q^3 dq      = -10
///    ∫ q dq               = 2

///Summing contributions:
///    I = 16/7 - 10 + 16 - 10 + 2 = 2/7

///Normalization equation:
///    1 = 2π * C * h^2 * (2/7)
///    => C = 7 / (4π h^2)

///Final normalized 2D Wendland C2 kernel:
///    W(r,h) = (7 / (4π h^2)) * (1 - q/2)^4 * (2q + 1),   q = r/h, 0 ≤ q ≤ 2
///    W = 0 otherwise.
///------------------------------------------------------------------------------
public static class WendlandKernel
{
    private const double Alpha2D = 7.0 / (4.0 * Math.PI); // 2D normalization constant

    /// <summary>
    /// Computes the kernel value W(r, h).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double W(double r, double h)
    {
        double q = Math.Abs(r) / h;
        if (q >= 2.0) return 0.0;

        double factor = 1.0 - 0.5 * q;
        return Alpha2D / (h * h) * factor * factor * factor * factor * (2.0 * q + 1.0);
    }

    /// <summary>
    /// Computes the kernel gradient value ∇W(r, h).
    /// Returns the gradient as a scalar to be multiplied by the unit vector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GradW(double r, double h)
    {
        double absR = Math.Abs(r);
        if (absR < 1e-12) return 0.0; // Avoid division by zero

        double q = absR / h;
        if (q >= 2.0) return 0.0;

        double factor = 1.0 - 0.5 * q;
        // The gradient should point in the direction of r, so multiply by sign(r)
        double grad = -Alpha2D / (h * h * h) * 5.0 * q * factor * factor * factor;
        return Math.Sign(r) * grad;
    }

    /// <summary>
    /// Computes the kernel Laplacian value ∇²W(r, h) (used for viscosity calculations).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LaplacianW(double r, double h)
    {
        double q = Math.Abs(r) / h;
        if (q >= 2.0) return 0.0;

        double factor = 1.0 - 0.5 * q;
        double term1 = 5.0 * factor * factor * factor;
        double term2 = -7.5 * q * factor * factor;
        return Alpha2D / (h * h * h * h) * (term1 + term2);
    }
}