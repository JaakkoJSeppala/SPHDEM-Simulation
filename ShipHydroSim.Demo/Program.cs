using System;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.SPH;

namespace ShipHydroSim.Demo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== ShipHydroSim SPH Demo ===\n");

        // Create SPH solver
        var solver = new SPHSolver
        {
            SmoothingLength = 0.2,
            RestDensity = 1000.0,
            Stiffness = 20000.0,
            Viscosity = 0.05,
            Gravity = new Vector3(0, -9.81, 0),
            TimeStep = 0.001
        };

        // Create a dam break scenario: water column on the left
        Console.WriteLine("Creating dam break scenario...");
        int particleId = 0;
        double spacing = 0.15;
        
        for (double x = 1.0; x < 3.0; x += spacing)
        {
            for (double y = 0.1; y < 5.0; y += spacing)
            {
                for (double z = 1.0; z < 3.0; z += spacing)
                {
                    var particle = new Particle(particleId++, new Vector3(x, y, z), mass: 0.02)
                    {
                        Velocity = Vector3.Zero
                    };
                    solver.AddParticle(particle);
                }
            }
        }

        Console.WriteLine($"Created {solver.Particles.Count} particles\n");

        // Run simulation
        double simulationTime = 0.0;
        double maxTime = 3.0;
        int outputInterval = 500; // Output every 500 steps
        
        Console.WriteLine("Running simulation...");
        Console.WriteLine("Time(s)\tParticles\tAvgHeight\tAvgVel");
        Console.WriteLine("-------\t---------\t---------\t-------");

        int step = 0;
        while (simulationTime < maxTime)
        {
            solver.Step();
            simulationTime += solver.TimeStep;
            step++;

            if (step % outputInterval == 0)
            {
                var stats = ComputeStats(solver);
                Console.WriteLine($"{simulationTime:F3}\t{solver.Particles.Count}\t\t{stats.avgHeight:F3}\t\t{stats.avgVel:F3}");
            }
        }

        Console.WriteLine("\nSimulation complete!");
        Console.WriteLine($"Total steps: {step}");
        Console.WriteLine($"Final time: {simulationTime:F3} s");
        
        var finalStats = ComputeStats(solver);
        Console.WriteLine($"\nFinal statistics:");
        Console.WriteLine($"  Average height: {finalStats.avgHeight:F3} m");
        Console.WriteLine($"  Average velocity: {finalStats.avgVel:F3} m/s");
        Console.WriteLine($"  Average density: {finalStats.avgDensity:F1} kg/m³");
    }

    static (double avgHeight, double avgVel, double avgDensity) ComputeStats(SPHSolver solver)
    {
        double sumHeight = 0;
        double sumVel = 0;
        double sumDensity = 0;
        int count = 0;

        foreach (var p in solver.Particles)
        {
            if (!p.IsBoundary)
            {
                sumHeight += p.Position.Y;
                sumVel += p.Velocity.Length;
                sumDensity += p.Density;
                count++;
            }
        }

        return count > 0
            ? (sumHeight / count, sumVel / count, sumDensity / count)
            : (0, 0, 0);
    }
}
