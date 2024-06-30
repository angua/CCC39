using System.Numerics;

namespace CCC39Lib;

public class PathRectangle : Rectangle
{
    public PathRectangle(Vector2 upperLeftCorner, Vector2 lowerRightCorner) : base(upperLeftCorner, lowerRightCorner)
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

    public Vector2 FirstOppositePosition { get; set; }

    public List<Vector2> Path = new();

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
        if (FirstMoveDir.X < 0)
        {
            // move left first
            FirstOppositePosition = new Vector2(UpperLeftCornerX, StartPosition.Y);
            _firstMoveCount = Width - 1;
        }
        else if (FirstMoveDir.X > 0)
        {
            // move right first
            FirstOppositePosition = new Vector2(LowerRightCornerX, StartPosition.Y);
            _firstMoveCount = Width - 1;
        }
        else if (FirstMoveDir.Y < 0)
        {
            // move up first
            FirstOppositePosition = new Vector2(StartPosition.X, UpperLeftCornerY);
            _firstMoveCount = Height - 1;
        }
        else if (FirstMoveDir.Y > 0)
        {
            // move down first
            FirstOppositePosition = new Vector2(StartPosition.X, LowerRightCornerY);
            _firstMoveCount = Height - 1;
        }
        else
        {
            // both directions 0, end position is start position
            FirstOppositePosition = new Vector2(StartPosition.X, StartPosition.Y);
            _firstMoveCount = 0;
            _secondMoveCount = 0;
        }

        if (SecondMoveDir.Y != 0)
        {
            // move in x direction first, then meander in Y
            // after an odd number of meandering back and forth, the end position is at the opposite x position from the start
            // after even number, same x position as start
            var endX = Height % 2 != 0 ? FirstOppositePosition.X : StartPosition.X;
            var endY = StartPosition.Y == UpperLeftCornerY ? LowerRightCornerY : UpperLeftCornerY;

            EndPosition = new Vector2(endX, endY);
            _secondMoveCount = Height - 1;
        }
        else if (SecondMoveDir.X != 0)
        {
            // move in y first, the meander in x
            var endY = Width % 2 != 0 ? FirstOppositePosition.Y : StartPosition.Y;
            var endX = StartPosition.X == UpperLeftCornerX ? LowerRightCornerX : UpperLeftCornerX;
            EndPosition = new Vector2(endX, endY);
            _secondMoveCount = Width - 1;
        }
        else
        {
            // no mevement in second dir
            EndPosition = FirstOppositePosition;
            _secondMoveCount = 0;
        }

    }
}

