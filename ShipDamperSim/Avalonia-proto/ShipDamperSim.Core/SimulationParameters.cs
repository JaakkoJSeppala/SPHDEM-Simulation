using System.IO;
using System.Text.Json;
using System.Numerics;

namespace ShipDamperSim.Core;

/// <summary>
/// Sisältää kaikki simulaation parametrit (tankki, SPH, damperi, pakotus, jne.).
/// Ladataan suoraan config.json-tiedostosta.
/// </summary>
public class SimulationParameters
{
    /// <summary>
    /// Tankin mitat ja täyttö.
    /// </summary>
    public class TankParams
    {
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double FillLevel { get; set; }
    }
    /// <summary>
    /// SPH-mallin parametrit (resoluutio, tiheys, viskositeetti, jne.).
    /// </summary>
    public class SphParams
    {
        public double Resolution { get; set; }
        public double RestDensity { get; set; }
        public double Gamma { get; set; }
        public double C0 { get; set; }
        public double Mu { get; set; }
    }
    /// <summary>
    /// Damperin mitat, sijainti, täyttöaste ja raekoko.
    /// </summary>
    public class DamperParams
    {
        public bool Enabled { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Position3D Position { get; set; }
        public double FillFraction { get; set; }
        public double ParticleDiameter { get; set; }
        public double Density { get; set; }
    }
    /// <summary>
    /// 3D-koordinaatti (esim. damperin sijainti).
    /// </summary>
    public class Position3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
    /// <summary>
    /// Pakotuksen tyyppi ja parametrit (esim. sway, roll).
    /// </summary>
    public class ExcitationParams
    {
        public string Type { get; set; }
        public double Amplitude { get; set; }
        public double Frequency { get; set; }
    }

    public TankParams Tank { get; set; }
    public SphParams Sph { get; set; }
    public DamperParams Damper { get; set; }
    public ExcitationParams Excitation { get; set; }

    /// <summary>
    /// Lataa parametrit json-tiedostosta ja tarkistaa perusvirheet.
    /// </summary>
    public static SimulationParameters LoadFromJson(string path)
    {
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        var parameters = JsonSerializer.Deserialize<SimulationParameters>(json, options);
        if (parameters == null)
            throw new InvalidDataException("Parametrien luku epäonnistui: tiedosto tai rakenne virheellinen.");
        // Perustarkistuksia
        if (parameters.Tank == null || parameters.Sph == null || parameters.Damper == null)
            throw new InvalidDataException("Pakollisia parametreja puuttuu (Tank, Sph, Damper).");
        if (parameters.Tank.Length <= 0 || parameters.Tank.Width <= 0 || parameters.Tank.Height <= 0)
            throw new InvalidDataException("Tankin mitat eivät voi olla nollia tai negatiivisia.");
        if (parameters.Sph.Resolution <= 0)
            throw new InvalidDataException("SPH-resoluutio oltava positiivinen.");
        if (parameters.Damper.ParticleDiameter <= 0)
            throw new InvalidDataException("Damperin raekoko oltava positiivinen.");
        return parameters;
    }
}
