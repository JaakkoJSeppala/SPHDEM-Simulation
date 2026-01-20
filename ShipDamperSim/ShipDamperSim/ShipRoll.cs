namespace ShipDamperSim;

public sealed class ShipRoll
{
    private readonly double _I, _c, _k, _mass;

    public double Phi { get; private set; }
    public double PhiDot { get; private set; }
    public double Y { get; private set; } // heave
    public double YDot { get; private set; }

    public ShipRoll(ShipConfig cfg)
    {
        _I = cfg.Inertia;
        _c = cfg.HydroDamping;
        _k = cfg.Restoring;
        _mass = 100.0; // laivan massa (kg), TODO: configista

        Phi = Util.Deg2Rad(cfg.Phi0Deg);
        PhiDot = Util.Deg2Rad(cfg.PhiDot0DegPerS);
        Y = 1.0; // aloituskorkeus (m)
        YDot = 0.0;
    }

    public void Step(double dt, double mWave, double mDamper, double forceY)
    {
        // Roll
        double phiDDot = (mWave + mDamper - _c * PhiDot - _k * Phi) / _I;
        PhiDot += dt * phiDDot;
        Phi += dt * PhiDot;
        // Heave (Y)
        double g = 9.81;
        double yDDot = (forceY - _mass * g) / _mass;
        YDot += dt * yDDot;
        Y += dt * YDot;
    }
}
