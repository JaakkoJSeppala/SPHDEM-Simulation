using System;

namespace ShipHydroSim.Core.SPH;

/// <summary>
/// SPH kernel functions for smoothing
/// </summary>
public static class KernelFunctions
{
    /// <summary>
    /// Cubic spline kernel (M4 B-spline)
    /// </summary>
    public static double CubicSpline(double r, double h)
    {
        double q = r / h;
        double factor = 1.0 / (Math.PI * h * h * h);

        if (q >= 0 && q < 1)
            return factor * (1.0 - 1.5 * q * q + 0.75 * q * q * q);
        else if (q >= 1 && q < 2)
        {
            double temp = 2.0 - q;
            return factor * 0.25 * temp * temp * temp;
        }
        return 0.0;
    }

    /// <summary>
    /// Gradient of cubic spline kernel
    /// </summary>
    public static double CubicSplineGradient(double r, double h)
    {
        if (r < 1e-10) return 0.0;

        double q = r / h;
        double factor = 1.0 / (Math.PI * h * h * h * h);

        if (q >= 0 && q < 1)
            return factor * (-3.0 * q + 2.25 * q * q);
        else if (q >= 1 && q < 2)
        {
            double temp = 2.0 - q;
            return factor * (-0.75 * temp * temp);
        }
        return 0.0;
    }

    /// <summary>
    /// Wendland C2 kernel (compact support, computationally efficient)
    /// </summary>
    public static double WendlandC2(double r, double h)
    {
        double q = r / h;
        if (q >= 2.0) return 0.0;

        double factor = 7.0 / (4.0 * Math.PI * h * h * h);
        double temp = 1.0 - 0.5 * q;
        return factor * temp * temp * temp * temp * (2.0 * q + 1.0);
    }

    /// <summary>
    /// Gradient of Wendland C2 kernel
    /// </summary>
    public static double WendlandC2Gradient(double r, double h)
    {
        if (r < 1e-10) return 0.0;

        double q = r / h;
        if (q >= 2.0) return 0.0;

        double factor = 7.0 / (4.0 * Math.PI * h * h * h * h);
        double temp = 1.0 - 0.5 * q;
        return factor * (-5.0 * q * temp * temp * temp);
    }
}
