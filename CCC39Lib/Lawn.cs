using System.Numerics;

namespace CCC39Lib;

public class Lawn
{
    public int Width { get; set; }
    public int Height { get; set; }

    public List<Vector2> TreePositions { get; set; } = new();

    // series of xy positions on the lawn
    public List<Vector2> Path { get; set; } = new();

    // movement instructions
    public List<char> Instructions { get; set; } = new();

}