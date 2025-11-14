using System;
using System.Collections.Generic;
using ShipHydroSim.Core.Geometry;
using ShipHydroSim.Core.SPH;

namespace ShipHydroSim.Core.Spatial;

/// <summary>
/// Spatial grid hash for efficient neighbor search in SPH
/// O(1) insertion, O(N) query where N is local density
/// </summary>
public class SpatialHash
{
    private readonly double _cellSize;
    private readonly Dictionary<(int, int, int), List<Particle>> _grid;

    public SpatialHash(double cellSize)
    {
        _cellSize = cellSize;
        _grid = new Dictionary<(int, int, int), List<Particle>>();
    }

    public void Clear()
    {
        foreach (var cell in _grid.Values)
            cell.Clear();
    }

    public void Insert(Particle particle)
    {
        var key = GetCellKey(particle.Position);
        if (!_grid.ContainsKey(key))
            _grid[key] = new List<Particle>();
        _grid[key].Add(particle);
    }

    /// <summary>
    /// Find all particles within radius of given position
    /// </summary>
    public List<Particle> FindNeighbors(Vector3 position, double radius)
    {
        var neighbors = new List<Particle>();
        double radiusSq = radius * radius;

        // Check all cells within the search radius
        int cellRadius = (int)Math.Ceiling(radius / _cellSize);
        var centerKey = GetCellKey(position);

        for (int i = -cellRadius; i <= cellRadius; i++)
        {
            for (int j = -cellRadius; j <= cellRadius; j++)
            {
                for (int k = -cellRadius; k <= cellRadius; k++)
                {
                    var key = (centerKey.Item1 + i, centerKey.Item2 + j, centerKey.Item3 + k);
                    if (_grid.TryGetValue(key, out var cell))
                    {
                        foreach (var particle in cell)
                        {
                            double distSq = (particle.Position - position).LengthSquared;
                            if (distSq < radiusSq)
                                neighbors.Add(particle);
                        }
                    }
                }
            }
        }

        return neighbors;
    }

    private (int, int, int) GetCellKey(Vector3 position)
    {
        return (
            (int)Math.Floor(position.X / _cellSize),
            (int)Math.Floor(position.Y / _cellSize),
            (int)Math.Floor(position.Z / _cellSize)
        );
    }
}
