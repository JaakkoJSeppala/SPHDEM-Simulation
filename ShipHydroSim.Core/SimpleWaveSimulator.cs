using System;

namespace ShipHydroSim.Core;

public class SimpleWaveSimulator
{
    private readonly HeightField _field;
    private double _time;

    public SimpleWaveSimulator(HeightField field)
    {
        _field = field;
    }

    public void Step(double dt)
    {
        _time += dt;
        for (int i = 0; i < _field.Nx; i++)
        {
            for (int j = 0; j < _field.Ny; j++)
            {
                double x = (double)i / (_field.Nx - 1) * 10.0;
                double y = (double)j / (_field.Ny - 1) * 10.0;

                _field.H[i, j] =
                    0.3 * Math.Sin(x + _time) +
                    0.2 * Math.Sin(0.7 * y - 1.3 * _time);
            }
        }
    }

    public HeightField Field => _field;
}
