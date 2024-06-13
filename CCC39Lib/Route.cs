using System.Numerics;

namespace CCC39Lib;

internal class Route
{
    public List<Vector2> Positions { get; set; } = new();

    public int Length => Positions.Count;
}
