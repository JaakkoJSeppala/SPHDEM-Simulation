using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShipDamperSim;

public sealed class SimConfig
{
    // Optional: experiment type for automation ("single", "sweep", "roll_decay", "harmonic")
    public string? Experiment { get; set; }

    public required TimeConfig Time { get; set; }
    public required ShipConfig Ship { get; set; }
    public required DamperConfig Damper { get; set; }
    public required ExcitationConfig Excitation { get; set; }
    public required OutputConfig Output { get; set; }

    public double WaveAmplitude { get; set; } = 0.3; // Amplitude of moving wall (m)
    public double WaveFrequency { get; set; } = 0.5; // Frequency of moving wall (Hz)
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        WriteIndented = true
    };

    public SimConfig DeepClone()
    {
        var json = JsonSerializer.Serialize(this, JsonOptions);
        return JsonSerializer.Deserialize<SimConfig>(json, JsonOptions)!;
    }
}

public sealed class TimeConfig
{
    public double Dt { get; set; } = 1e-4;
    public double TEnd { get; set; } = 20.0;
    public int LogEvery { get; set; } = 10;
    public int Seed { get; set; } = 1;
}

public sealed class ShipConfig
{
    public double Inertia { get; set; } = 1.0;
    public double HydroDamping { get; set; } = 0.2;
    public double Restoring { get; set; } = 2.0;
    public double Phi0Deg { get; set; } = 10.0;
    public double PhiDot0DegPerS { get; set; } = 0.0;
}

public sealed class ExcitationConfig
{
    public string Type { get; set; } = "none";
    public double M0 { get; set; } = 0.0;
    public double FreqHz { get; set; } = 0.5;
}

public sealed class OutputConfig
{
    public string OutputDir { get; set; } = "out";
    public string CsvFile { get; set; } = "timeseries.csv";
}

public sealed class DamperConfig
{
    public bool Enabled { get; set; } = true;
    public double CenterY { get; set; } = 0.8;
    public double CenterZ { get; set; } = 0.6;
    public double SizeY { get; set; } = 0.6;
    public double SizeZ { get; set; } = 0.4;

    public int ParticleCount { get; set; } = 300; // Will be set based on FillRatio
    public double FillRatio { get; set; } = 0.5; // Fraction of container volume filled with grains
    public double Radius { get; set; } = 0.02;
    public double Density { get; set; } = 2500.0;

    public double Kn { get; set; } = 2e5;
    public double GammaN { get; set; } = 50.0; // Normal damping (Ns/m)
    public double Mu { get; set; } = 0.4;
    public double Kt { get; set; } = 1e5;
    public double Gt { get; set; } = 20.0;

    public double MaxParticleSpeed { get; set; } = 20.0;
}
