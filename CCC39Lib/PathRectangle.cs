using System.Numerics;

namespace CCC39Lib;

public class PathRectangle : Rectangle
{
    public PathRectangle(Vector2 vector1, Vector2 vector2) : base(vector1, vector2)
    { }

    public PathRectangle(int leftX, int rightX, int topY, int bottomY) : base(leftX, rightX, topY, bottomY)
    { }


    public PathRectangle(PathRectangle rect) : base(rect)
    {
        StartPosition = rect.StartPosition;
        EndPosition = rect.EndPosition;
        FirstMoveDir = rect.FirstMoveDir;
        SecondMoveDir = rect.SecondMoveDir;
    }

    public Vector2 StartPosition { get; set; }
    public Vector2 EndPosition { get; set; }
    public Vector2 FirstMoveDir { get; set; }
    public Vector2 SecondMoveDir { get; set; } = new Vector2(0, 0);

    private int _firstMoveCount = 0;
    private int _secondMoveCount = 0;

    public List<Vector2> Path { get; set; } = new();

    internal void CreatePath()
    {
        if (Path.Count == 0)
        {
            Path.Add(StartPosition);

            var currentPos = StartPosition;

            var second = 0;

            while (currentPos != EndPosition)
            {
                var firstMoveDir = second % 2 == 0 ? FirstMoveDir : -1 * FirstMoveDir;
                for (int first = 0; first < _firstMoveCount; first++)
                {
                    currentPos += firstMoveDir;
                    Path.Add(currentPos);
                }
                if (second < _secondMoveCount)
                {
                    currentPos += SecondMoveDir;
                    Path.Add(currentPos);
                }
                else
                {
                    break;
                }
                second++;
            }

        }
    }

    internal void GetEndPosition()
    {
        var firstOppositePosition = new Vector2(StartPosition.X, StartPosition.Y);

        if (FirstMoveDir.X < 0)
        {
            // move left first
            firstOppositePosition = new Vector2(LeftX, StartPosition.Y);
            _firstMoveCount = Width - 1;
        }
        else if (FirstMoveDir.X > 0)
        {
            // move right first
            firstOppositePosition = new Vector2(RightX, StartPosition.Y);
            _firstMoveCount = Width - 1;
        }
        else if (FirstMoveDir.Y < 0)
        {
            // move up first
            firstOppositePosition = new Vector2(StartPosition.X, TopY);
            _firstMoveCount = Height - 1;
        }
        else if (FirstMoveDir.Y > 0)
        {
            // move down first
            firstOppositePosition = new Vector2(StartPosition.X, BottomY);
            _firstMoveCount = Height - 1;
        }
        else
        {
            // both directions 0, end position is start position
            _firstMoveCount = 0;
            _secondMoveCount = 0;
        }

        if (SecondMoveDir.Y != 0)
        {
            // move in x direction first, then meander in Y
            // after an odd number of meandering back and forth, the end position is at the opposite x position from the start
            // after even number, same x position as start
            var endX = Height % 2 != 0 ? firstOppositePosition.X : StartPosition.X;
            var endY = StartPosition.Y == TopY ? BottomY : TopY;

            EndPosition = new Vector2(endX, endY);
            _secondMoveCount = Height - 1;
        }
        else if (SecondMoveDir.X != 0)
        {
            // move in y first, the meander in x
            var endY = Width % 2 != 0 ? firstOppositePosition.Y : StartPosition.Y;
            var endX = StartPosition.X == LeftX ? RightX : LeftX;
            EndPosition = new Vector2(endX, endY);
            _secondMoveCount = Width - 1;
        }
        else
        {
            // no mevement in second dir
            EndPosition = firstOppositePosition;
            _secondMoveCount = 0;
        }

    }
}

