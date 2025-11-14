using System;

namespace ShipHydroSim.Core.Geometry;

/// <summary>
/// 3x3 matrix for inertia tensors and rotations
/// </summary>
public struct Matrix3x3
{
    public double M00, M01, M02;
    public double M10, M11, M12;
    public double M20, M21, M22;

    public Matrix3x3(
        double m00, double m01, double m02,
        double m10, double m11, double m12,
        double m20, double m21, double m22)
    {
        M00 = m00; M01 = m01; M02 = m02;
        M10 = m10; M11 = m11; M12 = m12;
        M20 = m20; M21 = m21; M22 = m22;
    }

    public static Matrix3x3 Identity => new(
        1, 0, 0,
        0, 1, 0,
        0, 0, 1
    );

    public static Matrix3x3 Diagonal(double d0, double d1, double d2) => new(
        d0, 0, 0,
        0, d1, 0,
        0, 0, d2
    );

    public static Matrix3x3 operator +(Matrix3x3 a, Matrix3x3 b) => new(
        a.M00 + b.M00, a.M01 + b.M01, a.M02 + b.M02,
        a.M10 + b.M10, a.M11 + b.M11, a.M12 + b.M12,
        a.M20 + b.M20, a.M21 + b.M21, a.M22 + b.M22
    );

    public static Vector3 operator *(Matrix3x3 m, Vector3 v) => new(
        m.M00 * v.X + m.M01 * v.Y + m.M02 * v.Z,
        m.M10 * v.X + m.M11 * v.Y + m.M12 * v.Z,
        m.M20 * v.X + m.M21 * v.Y + m.M22 * v.Z
    );

    public Matrix3x3 Transpose() => new(
        M00, M10, M20,
        M01, M11, M21,
        M02, M12, M22
    );

    public override string ToString() => 
        $"[{M00:F2} {M01:F2} {M02:F2}]\n" +
        $"[{M10:F2} {M11:F2} {M12:F2}]\n" +
        $"[{M20:F2} {M21:F2} {M22:F2}]";
}
