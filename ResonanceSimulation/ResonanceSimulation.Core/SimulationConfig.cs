namespace ResonanceSimulation.Core;

/// <summary>
/// Simulaation konfiguraatio (parametrit).
/// </summary>
public class SimulationConfig
{
    // Aikainteg parameters
    public double TimeStep { get; set; } = 0.0001; // 0.1 ms
    public double TotalTime { get; set; } = 30.0;  // 30 s
    public int OutputInterval { get; set; } = 100;  // Tallenna joka 100. askel

    // Tankki (1:50 Aframax)
    public double TankWidth { get; set; } = 0.30;   // 15 m → 0.30 m
    public double TankHeight { get; set; } = 0.40;  // 20 m → 0.40 m
    public double FillRatio { get; set; } = 0.50;   // 50%

    // Liike
    public double Amplitude { get; set; } = 0.02;   // 2 cm
    public double Frequency { get; set; } = 0.6;    // 0.6 Hz (resonanssi)

    // SPH-parametrit
    public double SmoothingLength { get; set; } = 0.01;    // 1 cm
    public double ParticleSpacing { get; set; } = 0.005;   // 5 mm
    public double RestDensity { get; set; } = 1000.0;      // kg/m³ (vesi)
    public double Stiffness { get; set; } = 20000.0;       // Pa (WCSPH)
    public double Viscosity { get; set; } = 0.001;         // Pa·s (vesi)

    // DEM-parametrit
    public bool EnableDamper { get; set; } = true;
    public double ParticleDiameter { get; set; } = 0.005;  // 5 mm
    public double ParticleDensity { get; set; } = 2500.0;  // kg/m³ (hiekka/lasi)
    public double DamperMassRatio { get; set; } = 0.12;    // 12% nesteen massasta
    public double DamperHeight { get; set; } = 0.03;       // 3 cm pohjakotelo

    // Materiaaliparametrit (DEM)
    public double YoungsModulus { get; set; } = 1e7;       // Pa
    public double PoissonRatio { get; set; } = 0.3;        // -
    public double RestitutionCoeff { get; set; } = 0.5;    // -
    public double FrictionCoeff { get; set; } = 0.3;       // -

    // Painovoima
    public Vector2D Gravity { get; set; } = new Vector2D(0, -9.81); // m/s²
}
