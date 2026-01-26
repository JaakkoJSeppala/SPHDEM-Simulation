namespace ShipDamperSim;

public readonly struct Vec3
{
    public static Vec3 operator -(Vec3 a) => new(-a.X, -a.Y, -a.Z);
    public readonly double X, Y, Z;
    public Vec3(double x, double y, double z) { X = x; Y = y; Z = z; }

    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(double s, Vec3 a) => new(s * a.X, s * a.Y, s * a.Z);
    public static Vec3 operator *(Vec3 a, double s) => new(s * a.X, s * a.Y, s * a.Z);
    public static Vec3 operator /(Vec3 a, double s) => new(a.X / s, a.Y / s, a.Z / s);

    public double Dot(Vec3 b) => X * b.X + Y * b.Y + Z * b.Z;
    public double Norm2() => Dot(this);
    public double Norm() => Math.Sqrt(Norm2());

    public Vec3 Normalized()
    {
        double n = Norm();
        return n > 1e-12 ? this / n : new Vec3(0, 0, 0);
    }
}

public static class Util
{
    public static double Clamp(double x, double lo, double hi) => x < lo ? lo : (x > hi ? hi : x);
    public static double Deg2Rad(double deg) => deg * Math.PI / 180.0;
}
