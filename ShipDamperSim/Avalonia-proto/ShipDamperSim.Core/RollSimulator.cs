using System.Numerics;

namespace ShipDamperSim.Core
{
    public class RollSimulator
    {
        public float Angle; // radians
        public float AngularVelocity;
        public float Inertia = 1000f; // kg m^2
        public float HydroDamping = 100f; // Nms/rad
        public float Restoring = 5000f; // Nm/rad

        public void Step(float dt, float damperMoment)
        {
            // Yksinkertainen Euler-integraatio
            float hydro = -HydroDamping * AngularVelocity;
            float restoring = -Restoring * Angle;
            float totalMoment = hydro + restoring + damperMoment;
            float angularAcc = totalMoment / Inertia;
            AngularVelocity += angularAcc * dt;
            Angle += AngularVelocity * dt;
        }
    }
}
