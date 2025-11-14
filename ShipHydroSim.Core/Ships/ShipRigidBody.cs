using System;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.DEM;

namespace ShipHydroSim.Core.Ships;

/// <summary>
/// Ship rigid body with 6 degrees of freedom (6DOF)
/// Surge, Sway, Heave (translation) + Roll, Pitch, Yaw (rotation)
/// </summary>
public class ShipRigidBody : RigidBody
{
    // Ship-specific properties
    public double Length { get; set; }           // L (m)
    public double Beam { get; set; }             // B (m)
    public double Draft { get; set; }            // T (m)
    public double Displacement { get; set; }     // Volume (m³)
    public double BlockCoefficient { get; set; } = 0.7; // Cb ~ 0.6-0.85 typically
    
    // Hydrostatics
    public double GM_Roll { get; set; } = 0.5;    // Metacentric height in roll (m)
    public double GM_Pitch { get; set; } = 0.3;   // Metacentric height in pitch (m)
    public Vector3 CenterOfGravityOffset { get; set; } = Vector3.Zero; // Relative offset from geometric center
    
    // 6DOF motion
    public double Surge { get; set; }            // x-translation
    public double Sway { get; set; }             // y-translation
    public double Heave { get; set; }            // z-translation
    public double Roll { get; set; }             // x-rotation (rad)
    public double Pitch { get; set; }            // y-rotation (rad)
    public double Yaw { get; set; }              // z-rotation (rad)
    
    // Hydrodynamic coefficients
    public double AddedMass { get; set; } = 0.2; // Fraction of ship mass
    public double DampingCoeff { get; set; } = 0.1;
    
    public ShipRigidBody(int id, Vector3 position, double length, double beam, double draft) 
        : base(id, position, 0, new BoxShape(length, draft, beam))
    {
        Length = length;
        Beam = beam;
        Draft = draft;
        Displacement = length * beam * draft * BlockCoefficient;
        
        // Mass from Archimedes (assume steel hull, partially filled)
        double rhoWater = 1000.0;
        Mass = rhoWater * Displacement;
        InverseMass = 1.0 / Mass;
        
        // Inertia tensor for box-like hull
        // Axes: X = length (longitudinal), Y = up (vertical), Z = beam (transverse)
        // Roll about X, Pitch about Z, Yaw about Y
        double Ixx = Mass * (beam * beam + draft * draft) / 12.0;            // Roll
        double Iyy = Mass * (length * length + beam * beam) / 12.0;          // Yaw
        double Izz = Mass * (length * length + draft * draft) / 12.0;        // Pitch
        
        InertiaTensor = new Matrix3x3(
            Ixx, 0, 0,
            0, Iyy, 0,
            0, 0, Izz
        );
        
        InverseInertiaTensor = new Matrix3x3(
            1.0 / Ixx, 0, 0,
            0, 1.0 / Iyy, 0,
            0, 0, 1.0 / Izz
        );
    }
    
    /// <summary>
    /// Calculate buoyancy force (Archimedes principle)
    /// </summary>
    public Vector3 CalculateBuoyancy(double waterLevel)
    {
        double rhoWater = 1000.0;
        double g = 9.81;
        
        // Calculate submerged portion of hull
        double hullBottom = Position.Y - Draft / 2;
        double hullTop = Position.Y + Draft / 2;
        
        if (waterLevel <= hullBottom)
            return Vector3.Zero; // Completely above water
        
        if (waterLevel >= hullTop)
        {
            // Completely submerged
            double totalSubmergedVolume = Length * Beam * Draft;
            return new Vector3(0, rhoWater * g * totalSubmergedVolume, 0);
        }
        
        // Partially submerged - more accurate calculation
        double submergedDepth = waterLevel - hullBottom;
        double submergedVolume = Length * Beam * submergedDepth * BlockCoefficient;
        
        // Buoyancy acts at center of submerged volume
        return new Vector3(0, rhoWater * g * submergedVolume, 0);
    }

    /// <summary>
    /// Compute hydrostatic restoring torque for small angles using GM approximation.
    /// τ = -Δ * g * GM * θ  (for roll and pitch separately)
    /// </summary>
    public Vector3 CalculateHydrostaticRestoringTorque(double waterLevel)
    {
        double rhoWater = 1000.0;
        double g = 9.81;
        double hullBottom = Position.Y - Draft / 2;
        double hullTop = Position.Y + Draft / 2;
        if (waterLevel <= hullBottom) return Vector3.Zero; // Out of water – no hydrostatic torque
        // Approximate submerged fraction for partial submergence
        double submergedFraction = Math.Clamp((waterLevel - hullBottom) / Draft, 0.0, 1.0);
        double displacement = rhoWater * g * (Length * Beam * Draft * submergedFraction);
        // Use body up vector tilt to avoid Euler ambiguity (small-angle approximation)
        var up = Orientation.Rotate(new Vector3(0, 1, 0));
        // For small angles: up.Z ≈ roll angle (about X), up.X ≈ -pitch angle (about Z)
        double tauRoll = -displacement * GM_Roll * up.Z;          // about X
        double tauPitch =  displacement * GM_Pitch * up.X;        // about Z (note sign)
        return new Vector3(tauRoll, 0, tauPitch);
    }

    private (double roll, double pitch) GetRollPitchAngles()
    {
        // Assuming Y-up, quaternion to Euler (roll around X, pitch around Z if yaw around Y)
        // Standard aerospace yaw(Z), pitch(Y), roll(X) differs; we will treat small angle extraction by vector components.
        // Use quaternion to rotation matrix then angles.
        var q = Orientation;
        double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
        double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        double roll = Math.Atan2(sinr_cosp, cosr_cosp);
        double sinp = 2 * (q.W * q.Y - q.Z * q.X);
        double pitch;
        if (Math.Abs(sinp) >= 1)
            pitch = Math.CopySign(Math.PI / 2, sinp);
        else
            pitch = Math.Asin(sinp);
        return (roll, pitch);
    }
}

/// <summary>
/// Box collision shape for ships
/// </summary>
public class BoxShape : IShape
{
    public double Length { get; set; }
    public double Height { get; set; }
    public double Width { get; set; }
    
    public BoxShape(double length, double height, double width)
    {
        Length = length;
        Height = height;
        Width = width;
    }
    
    public double GetBoundingRadius()
    {
        return Math.Sqrt(Length * Length + Height * Height + Width * Width) / 2.0;
    }
}
