namespace ResonanceSimulation.Core;

/// <summary>
/// Tankin geometria ja sinimuotoinen liike.
/// Toteuttaa 1:50 skaalan Aframax-ballastitankin.
/// </summary>
public class TankGeometry
{
    // Tankin rajat (staattinen koordinaatisto)
    public double Width { get; init; }
    public double Height { get; init; }
    public double MinX => -Width / 2.0;
    public double MaxX => Width / 2.0;
    public double MinY { get; init; } = 0.0;
    public double MaxY => MinY + Height;

    // Liikeparametrit
    public double Amplitude { get; init; }
    public double Frequency { get; init; }

    // Nykyinen siirtymä
    public double CurrentDisplacement { get; private set; }
    public double CurrentVelocity { get; private set; }
    public double CurrentAcceleration { get; private set; }

    public TankGeometry(double width, double height, double amplitude, double frequency)
    {
        Width = width;
        Height = height;
        Amplitude = amplitude;
        Frequency = frequency;
    }

    /// <summary>
    /// Päivitä tankin siirtymä sinimuotoisella liikkeellä.
    /// x(t) = A sin(2πft)
    /// v(t) = A(2πf) cos(2πft)
    /// a(t) = -A(2πf)² sin(2πft)
    /// </summary>
    public void UpdateMotion(double time)
    {
        double omega = 2.0 * Math.PI * Frequency;
        double phase = omega * time;

        CurrentDisplacement = Amplitude * Math.Sin(phase);
        CurrentVelocity = Amplitude * omega * Math.Cos(phase);
        CurrentAcceleration = -Amplitude * omega * omega * Math.Sin(phase);
    }

    /// <summary>
    /// Muunna paikallinen koordinaatti globaaliksi (liikkuva tankki).
    /// </summary>
    public Vector2D LocalToGlobal(Vector2D localPosition)
    {
        return new Vector2D(localPosition.X + CurrentDisplacement, localPosition.Y);
    }

    /// <summary>
    /// Muunna globaali koordinaatti paikalliseksi.
    /// </summary>
    public Vector2D GlobalToLocal(Vector2D globalPosition)
    {
        return new Vector2D(globalPosition.X - CurrentDisplacement, globalPosition.Y);
    }

    /// <summary>
    /// Tarkista onko piste tankin sisällä (paikallisissa koordinaateissa).
    /// </summary>
    public bool IsInside(Vector2D localPosition)
    {
        return localPosition.X >= MinX && localPosition.X <= MaxX &&
               localPosition.Y >= MinY && localPosition.Y <= MaxY;
    }
}
