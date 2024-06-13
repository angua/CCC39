using System.Numerics;

namespace CCC39Lib;

public class Lawn
{
    public int Width { get; set; }
    public int Height { get; set; }

    public List<Vector2> TreePositions { get; set; } = new();

}