namespace ResonanceSimulation.Core;

/// <summary>
/// Mittausten teko simulaation aikana.
/// Tallentaa p(t), h(t), Ek(t) yms. dataa.
/// </summary>
public class Measurements
{
    public List<double> Times { get; } = new();
    public List<double> WallPressures { get; } = new();
    public List<double> FreeSurfaceHeights { get; } = new();
    public List<double> KineticEnergies { get; } = new();
    public List<double> TankDisplacements { get; } = new();

    private readonly TankGeometry _tank;
    private readonly SimulationConfig _config;

    // Maksimit (resonanssikäyrää varten)
    public double MaxWallPressure { get; private set; }
    public double MaxFreeSurfaceHeight { get; private set; }

    public Measurements(TankGeometry tank, SimulationConfig config)
    {
        _tank = tank;
        _config = config;
    }

    /// <summary>
    /// Mittaa seinämäpaine oikealla seinällä (tankin paikallisissa koordinaateissa).
    /// Käyttää SPH-partikkelien painetta.
    /// </summary>
    public void MeasureWallPressure(List<SPHParticle> particles, double time)
    {
        // Etsi partikkelit oikean seinän läheisyydestä
        double wallX = _tank.MaxX;
        double sampleDistance = 2.0 * _config.SmoothingLength;

        var nearWall = particles.Where(p =>
        {
            var localPos = _tank.GlobalToLocal(p.Position);
            return Math.Abs(localPos.X - wallX) < sampleDistance;
        }).ToList();

        double avgPressure = nearWall.Any() ? nearWall.Average(p => p.Pressure) : 0.0;

        Times.Add(time);
        WallPressures.Add(avgPressure);
        TankDisplacements.Add(_tank.CurrentDisplacement);

        MaxWallPressure = Math.Max(MaxWallPressure, avgPressure);
    }

    /// <summary>
    /// Mittaa vapaan pinnan korkeus (maksimikoordinaatti Y-suunnassa).
    /// </summary>
    public void MeasureFreeSurface(List<SPHParticle> particles, double time)
    {
        if (!particles.Any()) return;

        double maxHeight = particles.Max(p =>
        {
            var localPos = _tank.GlobalToLocal(p.Position);
            return localPos.Y;
        });

        FreeSurfaceHeights.Add(maxHeight);
        MaxFreeSurfaceHeight = Math.Max(MaxFreeSurfaceHeight, maxHeight);
    }

    /// <summary>
    /// Mittaa liike-energia (SPH + DEM).
    /// Ek = Σ (½ m v²)
    /// </summary>
    public void MeasureKineticEnergy(List<SPHParticle> sph, List<DEMParticle> dem, double time)
    {
        double sphEnergy = sph.Sum(p => 0.5 * p.Mass * p.Velocity.LengthSquared());
        double demEnergy = dem.Sum(p => 0.5 * p.Mass * p.Velocity.LengthSquared());
        double demRotational = dem.Sum(p => 0.5 * p.Inertia * p.AngularVelocity * p.AngularVelocity);

        double totalEnergy = sphEnergy + demEnergy + demRotational;
        KineticEnergies.Add(totalEnergy);
    }

    /// <summary>
    /// Laske vaimennussuhde ζ energian eksponentiaalisen pienenemisen perusteella.
    /// ζ = (1 / 4π) ln(A(t) / A(t+T))
    /// </summary>
    public double ComputeDampingRatio()
    {
        if (KineticEnergies.Count < 100) return 0.0;

        // Etsi piikkejä (paikallisia maksimeja)
        var peaks = new List<(int Index, double Energy)>();
        for (int i = 1; i < KineticEnergies.Count - 1; i++)
        {
            if (KineticEnergies[i] > KineticEnergies[i - 1] &&
                KineticEnergies[i] > KineticEnergies[i + 1] &&
                KineticEnergies[i] > 0.01) // Minimi kynnys
            {
                peaks.Add((i, KineticEnergies[i]));
            }
        }

        if (peaks.Count < 2) return 0.0;

        // Laske logaritminen dekrementti kahden ensimmäisen piikin välillä
        double delta = Math.Log(peaks[0].Energy / peaks[1].Energy);
        double zeta = delta / (2.0 * Math.PI);

        return zeta;
    }

    /// <summary>
    /// Tallenna mittaukset CSV:ksi.
    /// </summary>
    public void SaveToCSV(string filePath)
    {
        using var writer = new StreamWriter(filePath);
        writer.WriteLine("Time,WallPressure,FreeSurfaceHeight,KineticEnergy,TankDisplacement");

        for (int i = 0; i < Times.Count; i++)
        {
            writer.WriteLine($"{Times[i]:F6},{WallPressures[i]:F3},{FreeSurfaceHeights[i]:F6},{KineticEnergies[i]:F6},{TankDisplacements[i]:F6}");
        }
    }
}
