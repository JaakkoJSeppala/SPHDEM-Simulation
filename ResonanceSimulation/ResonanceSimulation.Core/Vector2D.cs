using System.Runtime.CompilerServices;

namespace ResonanceSimulation.Core;

/// <summary>
/// 2D vektori optimoitu SPH/DEM-laskentaan.
/// Käyttää struct-toteutusta cache-ystävällisyyden vuoksi.
/// </summary>
public readonly struct Vector2D : IEquatable<Vector2D>
{
    public readonly double X;
    public readonly double Y;

    public Vector2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator +(Vector2D a, Vector2D b) => new(a.X + b.X, a.Y + b.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator -(Vector2D a, Vector2D b) => new(a.X - b.X, a.Y - b.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator *(Vector2D v, double scalar) => new(v.X * scalar, v.Y * scalar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator *(double scalar, Vector2D v) => new(v.X * scalar, v.Y * scalar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator /(Vector2D v, double scalar) => new(v.X / scalar, v.Y / scalar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double LengthSquared() => X * X + Y * Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Length() => Math.Sqrt(LengthSquared());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator -(Vector2D v) => new(-v.X, -v.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2D Normalized()
    {
        double len = Length();
        return len > 1e-12 ? this / len : new Vector2D(0, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Vector2D a, Vector2D b) => a.X * b.X + a.Y * b.Y;

    public bool Equals(Vector2D other) => X.Equals(other.X) && Y.Equals(other.Y);
    public override bool Equals(object? obj) => obj is Vector2D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"({X:F6}, {Y:F6})";

    public static readonly Vector2D Zero = new(0, 0);
    public static readonly Vector2D UnitX = new(1, 0);
    public static readonly Vector2D UnitY = new(0, 1);
}
