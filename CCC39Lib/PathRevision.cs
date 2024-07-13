using System.Numerics;

namespace CCC39Lib;

public class PathRevision
{
    public List<Vector2> Path { get; set; } = new();

    public List<Vector2> ForbiddenExpandPositions { get; set; } = new();

    public Vector2 ExpandPosition { get; set; }

    public bool IsValid { get; set; } = true;

}
