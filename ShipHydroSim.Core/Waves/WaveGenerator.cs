using System;
using ShipHydroSim.Core.Geometry;

namespace ShipHydroSim.Core.Waves;

/// <summary>
/// Base interface for wave generators
/// </summary>
public interface IWaveGenerator
{
    double GetElevation(double x, double z, double time);
    Vector3 GetVelocity(double x, double y, double z, double time);
}

/// <summary>
/// Simple sinusoidal wave (linear wave theory)
/// </summary>
public class SineWave : IWaveGenerator
{
    public double Amplitude { get; set; } = 0.5;      // A (m)
    public double WaveLength { get; set; } = 5.0;     // λ (m)
    public double Period { get; set; } = 2.0;         // T (s)
    public Vector3 Direction { get; set; } = new(1, 0, 0); // Wave propagation direction
    
    public double GetElevation(double x, double z, double time)
    {
        double k = 2.0 * Math.PI / WaveLength;
        double omega = 2.0 * Math.PI / Period;
        double phase = k * (x * Direction.X + z * Direction.Z) - omega * time;
        return Amplitude * Math.Sin(phase);
    }
    
    public Vector3 GetVelocity(double x, double y, double z, double time)
    {
        double k = 2.0 * Math.PI / WaveLength;
        double omega = 2.0 * Math.PI / Period;
        double phase = k * (x * Direction.X + z * Direction.Z) - omega * time;
        
        // Linear wave theory velocity field
        double depth = 10.0; // Water depth (m)
        double factor = omega * Amplitude * Math.Cosh(k * (y + depth)) / Math.Sinh(k * depth);
        
        return new Vector3(
            factor * Math.Cos(phase) * Direction.X,
            omega * Amplitude * Math.Sinh(k * (y + depth)) / Math.Sinh(k * depth) * Math.Sin(phase),
            factor * Math.Cos(phase) * Direction.Z
        );
    }
}

/// <summary>
/// Stokes 2nd order wave (non-linear)
/// </summary>
public class StokesWave : IWaveGenerator
{
    public double Amplitude { get; set; } = 0.5;
    public double WaveLength { get; set; } = 5.0;
    public double Period { get; set; } = 2.0;
    public Vector3 Direction { get; set; } = new(1, 0, 0);
    
    public double GetElevation(double x, double z, double time)
    {
        double k = 2.0 * Math.PI / WaveLength;
        double omega = 2.0 * Math.PI / Period;
        double phase = k * (x * Direction.X + z * Direction.Z) - omega * time;
        
        // Stokes 2nd order
        double eta1 = Amplitude * Math.Sin(phase);
        double eta2 = k * Amplitude * Amplitude * Math.Cos(2 * phase) / 4.0;
        
        return eta1 + eta2;
    }
    
    public Vector3 GetVelocity(double x, double y, double z, double time)
    {
        // Simplified - use linear theory for now
        double k = 2.0 * Math.PI / WaveLength;
        double omega = 2.0 * Math.PI / Period;
        double phase = k * (x * Direction.X + z * Direction.Z) - omega * time;
        double depth = 10.0;
        
        double factor = omega * Amplitude * Math.Cosh(k * (y + depth)) / Math.Sinh(k * depth);
        
        return new Vector3(
            factor * Math.Cos(phase) * Direction.X,
            omega * Amplitude * Math.Sinh(k * (y + depth)) / Math.Sinh(k * depth) * Math.Sin(phase),
            factor * Math.Cos(phase) * Direction.Z
        );
    }
}

/// <summary>
/// Irregular wave using JONSWAP spectrum (realistic sea state)
/// </summary>
public class IrregularWave : IWaveGenerator
{
    private readonly WaveComponent[] _components;
    
    public IrregularWave(double significantWaveHeight, double peakPeriod, int numComponents = 30)
    {
        _components = new WaveComponent[numComponents];
        double peakFreq = 2.0 * Math.PI / peakPeriod;
        
        Random rng = new Random(42); // Fixed seed for reproducibility
        
        for (int i = 0; i < numComponents; i++)
        {
            double freq = peakFreq * (0.5 + 1.5 * i / numComponents);
            double omega = freq;
            double spectrum = JONSWAPSpectrum(omega, significantWaveHeight, peakFreq);
            
            _components[i] = new WaveComponent
            {
                Amplitude = Math.Sqrt(2 * spectrum * (1.5 * peakFreq / numComponents)),
                Omega = omega,
                Phase = rng.NextDouble() * 2 * Math.PI,
                Direction = new Vector3(Math.Cos(rng.NextDouble() * 0.5), 0, Math.Sin(rng.NextDouble() * 0.5))
            };
        }
    }
    
    public double GetElevation(double x, double z, double time)
    {
        double eta = 0;
        foreach (var comp in _components)
        {
            double k = comp.Omega * comp.Omega / 9.81; // Dispersion relation
            double phase = k * (x * comp.Direction.X + z * comp.Direction.Z) - comp.Omega * time + comp.Phase;
            eta += comp.Amplitude * Math.Cos(phase);
        }
        return eta;
    }
    
    public Vector3 GetVelocity(double x, double y, double z, double time)
    {
        Vector3 vel = Vector3.Zero;
        double depth = 10.0;
        
        foreach (var comp in _components)
        {
            double k = comp.Omega * comp.Omega / 9.81;
            double phase = k * (x * comp.Direction.X + z * comp.Direction.Z) - comp.Omega * time + comp.Phase;
            
            double factor = comp.Omega * comp.Amplitude * Math.Cosh(k * (y + depth)) / Math.Sinh(k * depth);
            
            vel += new Vector3(
                factor * Math.Sin(phase) * comp.Direction.X,
                comp.Omega * comp.Amplitude * Math.Sinh(k * (y + depth)) / Math.Sinh(k * depth) * Math.Cos(phase),
                factor * Math.Sin(phase) * comp.Direction.Z
            );
        }
        
        return vel;
    }
    
    private double JONSWAPSpectrum(double omega, double Hs, double omegaPeak)
    {
        double alpha = 0.0081; // JONSWAP constant
        double gamma = 3.3;    // Peak enhancement factor
        
        double sigma = omega <= omegaPeak ? 0.07 : 0.09;
        double r = Math.Exp(-Math.Pow(omega - omegaPeak, 2) / (2 * sigma * sigma * omegaPeak * omegaPeak));
        
        return alpha * 9.81 * 9.81 / Math.Pow(omega, 5) * 
               Math.Exp(-1.25 * Math.Pow(omegaPeak / omega, 4)) * 
               Math.Pow(gamma, r);
    }
    
    private struct WaveComponent
    {
        public double Amplitude;
        public double Omega;
        public double Phase;
        public Vector3 Direction;
    }
}
