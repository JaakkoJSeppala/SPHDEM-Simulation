using System;
using System.Collections.Generic;
using ShipHydroSim.Core.Geometry;

namespace ShipHydroSim.Core.Coupling;

/// <summary>
/// Generates virtual boundary particles on rigid body surfaces
/// for accurate SPH-DEM coupling
/// </summary>
public static class BoundaryShellGenerator
{
    /// <summary>
    /// Generate boundary particles on a box hull (simplified ship representation)
    /// Creates surface discretization with normals and areas
    /// </summary>
    /// <param name="length">Hull length (X direction)</param>
    /// <param name="width">Hull width (Z direction)</param>
    /// <param name="height">Hull height (Y direction, draft + freeboard)</param>
    /// <param name="spacing">Particle spacing on surface</param>
    /// <returns>List of boundary particles in body-local coordinates</returns>
    public static List<BoundaryParticle> GenerateBoxHull(
        double length, double width, double height, double spacing)
    {
        var particles = new List<BoundaryParticle>();
        
        double halfL = length * 0.5;
        double halfW = width * 0.5;
        double halfH = height * 0.5;
        
        // Bottom face (keel, Y = -halfH, normal = (0, -1, 0))
        AddRectangularFace(particles, 
            new Vector3(-halfL, -halfH, -halfW),
            new Vector3(length, 0, 0),
            new Vector3(0, 0, width),
            new Vector3(0, -1, 0),
            spacing);
        
        // Top face (deck, Y = halfH, normal = (0, 1, 0))
        AddRectangularFace(particles,
            new Vector3(-halfL, halfH, -halfW),
            new Vector3(length, 0, 0),
            new Vector3(0, 0, width),
            new Vector3(0, 1, 0),
            spacing);
        
        // Bow (front, X = halfL, normal = (1, 0, 0))
        AddRectangularFace(particles,
            new Vector3(halfL, -halfH, -halfW),
            new Vector3(0, height, 0),
            new Vector3(0, 0, width),
            new Vector3(1, 0, 0),
            spacing);
        
        // Stern (back, X = -halfL, normal = (-1, 0, 0))
        AddRectangularFace(particles,
            new Vector3(-halfL, -halfH, -halfW),
            new Vector3(0, height, 0),
            new Vector3(0, 0, width),
            new Vector3(-1, 0, 0),
            spacing);
        
        // Starboard (right, Z = halfW, normal = (0, 0, 1))
        AddRectangularFace(particles,
            new Vector3(-halfL, -halfH, halfW),
            new Vector3(length, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(0, 0, 1),
            spacing);
        
        // Port (left, Z = -halfW, normal = (0, 0, -1))
        AddRectangularFace(particles,
            new Vector3(-halfL, -halfH, -halfW),
            new Vector3(length, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(0, 0, -1),
            spacing);
        
        return particles;
    }
    
    /// <summary>
    /// Add boundary particles to a rectangular face
    /// </summary>
    /// <param name="particles">List to append to</param>
    /// <param name="corner">Starting corner position</param>
    /// <param name="u">First edge vector</param>
    /// <param name="v">Second edge vector</param>
    /// <param name="normal">Outward normal (unit vector)</param>
    /// <param name="spacing">Particle spacing</param>
    private static void AddRectangularFace(
        List<BoundaryParticle> particles,
        Vector3 corner,
        Vector3 u,
        Vector3 v,
        Vector3 normal,
        double spacing)
    {
        double uLen = u.Length;
        double vLen = v.Length;
        
        int nU = Math.Max(1, (int)(uLen / spacing));
        int nV = Math.Max(1, (int)(vLen / spacing));
        
        Vector3 uStep = u / nU;
        Vector3 vStep = v / nV;
        
        // Area per particle
        double totalArea = uLen * vLen;
        double particleArea = totalArea / (nU * nV);
        
        for (int i = 0; i < nU; i++)
        {
            for (int j = 0; j < nV; j++)
            {
                // Position at cell center
                Vector3 localPos = corner + uStep * (i + 0.5) + vStep * (j + 0.5);
                
                particles.Add(new BoundaryParticle(localPos, normal, particleArea));
            }
        }
    }
    
    /// <summary>
    /// Generate boundary particles on sphere surface (for testing)
    /// </summary>
    public static List<BoundaryParticle> GenerateSphere(double radius, int subdivisions)
    {
        var particles = new List<BoundaryParticle>();
        
        // Icosphere subdivision or simple lat-long grid
        int nTheta = subdivisions * 4;
        int nPhi = subdivisions * 2;
        
        double totalArea = 4.0 * Math.PI * radius * radius;
        double particleArea = totalArea / (nTheta * nPhi);
        
        for (int i = 0; i < nTheta; i++)
        {
            double theta = 2.0 * Math.PI * i / nTheta;
            
            for (int j = 0; j < nPhi; j++)
            {
                double phi = Math.PI * (j + 0.5) / nPhi; // [0, π]
                
                // Spherical to Cartesian
                double x = radius * Math.Sin(phi) * Math.Cos(theta);
                double y = radius * Math.Cos(phi);
                double z = radius * Math.Sin(phi) * Math.Sin(theta);
                
                Vector3 pos = new Vector3(x, y, z);
                Vector3 normal = pos.Normalized(); // For sphere, normal = position/radius
                
                particles.Add(new BoundaryParticle(pos, normal, particleArea));
            }
        }
        
        return particles;
    }
}
