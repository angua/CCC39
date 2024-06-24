using System.Numerics;

namespace CCC39Lib;

public class PathStep
{
    public PathRectangle PathingRectangle { get; set; }

    public List<Vector2> NextStartPositions { get; set; }
    public List<PathRectangle> NextRectangles { get; set; }
    public int NextRectangleIndex { get; set; }

    public List<Vector2> Path => PathingRectangle.Path;

    public bool IsValid { get; set; } = true;

    internal void CreatePath()
    {
        PathingRectangle.CreatePath();
    }
}
