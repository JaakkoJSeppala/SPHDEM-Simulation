namespace ShipHydroSim.Core;

public class HeightField
{
    public int Nx { get; }
    public int Ny { get; }
    public double[,] H { get; }

    public HeightField(int nx, int ny)
    {
        Nx = nx;
        Ny = ny;
        H = new double[nx, ny];
    }
}
