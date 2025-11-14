using System;

namespace ShipHydroSim.Core.Geometry;

/// <summary>
/// Quaternion for 3D rotations (used in DEM for rigid body orientation)
/// </summary>
public struct Quaternion
{
    public double W;
    public double X;
    public double Y;
    public double Z;

    public Quaternion(double w, double x, double y, double z)
    {
        W = w;
        X = x;
        Y = y;
        Z = z;
    }

    public static Quaternion Identity => new(1, 0, 0, 0);

    public double Length => Math.Sqrt(W * W + X * X + Y * Y + Z * Z);

    public Quaternion Normalized()
    {
        double len = Length;
        return len > 1e-10 ? new Quaternion(W / len, X / len, Y / len, Z / len) : Identity;
    }

    public static Quaternion operator *(Quaternion a, Quaternion b) => new(
        a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
        a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
        a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
        a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W
    );

    public Quaternion Conjugate() => new(W, -X, -Y, -Z);

    public Vector3 Rotate(Vector3 v)
    {
        // q * v * q^-1
        Quaternion vq = new(0, v.X, v.Y, v.Z);
        Quaternion result = this * vq * Conjugate();
        return new Vector3(result.X, result.Y, result.Z);
    }

    public static Quaternion FromAxisAngle(Vector3 axis, double angle)
    {
        double halfAngle = angle * 0.5;
        double s = Math.Sin(halfAngle);
        Vector3 a = axis.Normalized();
        return new Quaternion(Math.Cos(halfAngle), a.X * s, a.Y * s, a.Z * s);
    }

    public override string ToString() => $"Q({W:F3}, {X:F3}, {Y:F3}, {Z:F3})";
}
