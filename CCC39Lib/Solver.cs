using System.Drawing;
using System.Numerics;
using System.Text;
using Common;

namespace CCC39Lib;

public class Solver
{
    public string Solve(int level, List<string> lines)
    {
        return level switch
        {
            1 => SolveLevel1(lines),
            2 => SolveLevel2(lines),
            3 => SolveLevel3(lines),
            4 => SolveLevel4(lines),
            _ => throw new InvalidOperationException(($"Level {level} not supported."))
        };
    }

    private string SolveLevel1(List<string> lines)
    {
        var actualLines = lines.Skip(1);
        var fullResult = new StringBuilder();

        foreach (var line in actualLines)
        {
            var counts = new[]
            {
                line.Count(c => c == 'W'),
                line.Count(c => c == 'D'),
                line.Count(c => c == 'S'),
                line.Count(c => c == 'A'),
            };

            var resultLine = string.Join(' ', counts);
            fullResult.AppendLine(resultLine);
        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }


    private string SolveLevel2(List<string> lines)
    {
        var actualLines = lines.Skip(1);
        var fullResult = new StringBuilder();

        foreach (var line in actualLines)
        {
            var bounds = GetPathBounds(line, out _);
            var resultLine = string.Join(' ', new[] { (int)bounds.X, (int)bounds.Y });
            fullResult.AppendLine(resultLine);
        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }

    private Vector2 GetPathBounds(string path, out HashSet<Vector2> visitedPositions)
    {
        var currentPos = new Vector2(0, 0);

        visitedPositions = new HashSet<Vector2>();
        visitedPositions.Add(currentPos);

        var minX = 0;
        var minY = 0;
        var maxX = 0;
        var maxY = 0;

        foreach (var ch in path)
        {
            switch (ch)
            {
                case 'W': currentPos += new Vector2(0, -1); break;
                case 'A': currentPos += new Vector2(-1, 0); break;
                case 'S': currentPos += new Vector2(0, 1); break;
                case 'D': currentPos += new Vector2(1, 0); break;
            };

            minX = Math.Min((int)currentPos.X, minX);
            minY = Math.Min((int)currentPos.Y, minY);
            maxX = Math.Max((int)currentPos.X, maxX);
            maxY = Math.Max((int)currentPos.Y, maxY);

            if (visitedPositions.Contains(currentPos))
            {
                throw new InvalidOperationException("Path is crossing itself");
            }

            visitedPositions.Add(currentPos);
        }

        return new Vector2(maxX - minX + 1, maxY - minY + 1);
    }


    private string SolveLevel3(List<string> lines)
    {
        var fullResult = new StringBuilder();

        var numLawns = Convert.ToInt32(lines.First());

        int i = 1;
        while (i < lines.Count)
        {
            var isValid = EvaluatePath(lines, ref i);
            fullResult.AppendLine(isValid ? "VALID" : "INVALID");
        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }

    private bool EvaluatePath(List<string> lines, ref int i)
    {
        var dimensions = lines[i].Split(' ').Select(int.Parse).ToArray();
        var lawnLines = lines.Slice(i + 1, dimensions[1]).ToList();
        var instructions = lines[i + 1 + dimensions[1]];

        var width = dimensions[0];
        var height = dimensions[1];

        try
        {
            var pathBounds = GetPathBounds(instructions, out var visitedPositions);

            if (pathBounds.X != width || pathBounds.Y != height)
            {
                return false;
            }

            var minX = visitedPositions.Select(pos => pos.X).Min();
            var minY = visitedPositions.Select(pos => pos.Y).Min();

            var treePosY = lawnLines.FindIndex(line => line.Contains('X'));
            var treePosX = lawnLines[treePosY].IndexOf('X');

            var pathTreePos = new Vector2(minX + treePosX, minY + treePosY);

            if (visitedPositions.Contains(pathTreePos))
            {
                return false;
            }

            if (visitedPositions.Count() != width * height - 1) // minus the tree
            {
                return false;
            }
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        finally
        {
            i += dimensions[1] + 2; // skip instructions and dimensions
        }

        return true;
    }


    private string SolveLevel4(List<string> lines)
    {
        var fullResult = new StringBuilder();

        var numLawns = Convert.ToInt32(lines.First());

        var lawns = new List<Lawn>();

        var i = 1;
        while (i < lines.Count)
        {
            // new lawn
            var lawn = new Lawn();

            var line = lines[i];

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            lawn.Width = Convert.ToInt32(parts[0]);
            lawn.Height = Convert.ToInt32(parts[1]);

            for (int y = 0; y < lawn.Height; y++)
            {
                // parse tree positions
                i++;
                line = lines[i];

                for (int x = 0; x < lawn.Width; x++)
                {
                    if (line[x] == 'X')
                    {
                        lawn.TreePositions.Add(new Vector2(x, y));
                    }
                }
            }
            lawns.Add(lawn);

            //fullResult.AppendLine(FindPath(lawn));


            // next lawn
            i++;
        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }


    public void FindPath(Lawn lawn)
    {
        while (!lawn.MowingFinished)
        {
            FindPathNextStep(lawn);
        }
    }

    public void FindPathNextStep(Lawn lawn)
    {
        lawn.PathStepsCount++;

        // start pathing
        if (lawn.StartPositions.Count == 0)
        {
            // create start positions
            lawn.SetStartPositions();
            lawn.StartPositionIndex = 0;

            // create test rectangles from first start position
            var startPosition = lawn.StartPositions[lawn.StartPositionIndex];
            lawn.NextPathrectangles = FindRectangles(lawn, startPosition);
            lawn.NextRectangleIndex = 0;

            var nextRectangle = lawn.NextPathrectangles[lawn.NextRectangleIndex];
            CreatePathStep(lawn, nextRectangle);
        }
        else
        {
            // pathing already running
            GetNextStep(lawn);

        }

    }

    private void GetNextStep(Lawn lawn)
    {
        if (lawn.MowingFinished)
        {
            return;
        }

        var currentPathStep = lawn.PathSteps.Last();

        if (currentPathStep.IsValid)
        {
            if (currentPathStep.NextRectangleIndex >= currentPathStep.NextRectangles.Count)
            {
                // no more possible rectangles from here, remove last step
                currentPathStep.IsValid = false;
                GetNextStep(lawn);
            }
            else
            {
                // can continue from here, create next rectangle
                var nextrectangle = currentPathStep.NextRectangles[currentPathStep.NextRectangleIndex];

                if (nextrectangle.Area == 1 && currentPathStep.NextRectangles.Count != 1)
                {
                    // only use 1x1 rectangle when no other are available
                    currentPathStep.NextRectangleIndex++;
                    GetNextStep(lawn);

                }
                else
                {
                    CreatePathStep(lawn, nextrectangle);
                }
            }
        }
        else
        {
            // not valid, remove last rectangle
            RemovePathStep(lawn, currentPathStep);

            if (lawn.PathSteps.Count > 0)
            {
                var previousPathStep = lawn.PathSteps.Last();
                if (currentPathStep.ChooseDifferentPath)
                {
                    // don't try the other rectangles
                    previousPathStep.NextRectangleIndex = previousPathStep.NextRectangles.Count;
                }
                else
                {
                    // try next rectangle
                    previousPathStep.NextRectangleIndex++;
                }
                GetNextStep(lawn);
            }
            else
            {
                // No pathing steps on lawn. restart from start position
                lawn.NextRectangleIndex++;
                if (lawn.NextRectangleIndex >= lawn.NextPathrectangles.Count)
                {
                    // no more rectangles on this start position, take next start position
                    lawn.StartPositionIndex++;
                    var startPosition = lawn.StartPositions[lawn.StartPositionIndex];
                    lawn.NextPathrectangles = FindRectangles(lawn, startPosition);
                    lawn.NextRectangleIndex = 0;

                    var nextRectangle = lawn.NextPathrectangles[lawn.NextRectangleIndex];
                    CreatePathStep(lawn, nextRectangle);
                }
                else
                {
                    // try next rectangle
                    var nextRectangle = lawn.NextPathrectangles[lawn.NextRectangleIndex];
                    CreatePathStep(lawn, nextRectangle);
                }
            }

        }


    }

    private void RemovePathStep(Lawn lawn, PathStep currentPathStep)
    {
        lawn.PathRectangles.Remove(currentPathStep.PathingRectangle);
        lawn.ObjectsOnLawn.Remove(currentPathStep.PathingRectangle);
        lawn.PathSteps.Remove(currentPathStep);
    }

    private void CreatePathStep(Lawn lawn, PathRectangle rectangle)
    {
        var pathstep = new PathStep()
        {
            PathingRectangle = rectangle,
        };
        lawn.PathRectangles.Add(rectangle);
        lawn.PathSteps.Add(pathstep);
        lawn.ObjectsOnLawn.Add(rectangle);

        var coveredArea = lawn.GetCoveredArea();

        if (coveredArea == lawn.Fields)
        {
            // mowing finished
            lawn.MowingFinished = true;
            pathstep.IsValid = true;
            return;
        }

        pathstep.NextStartPositions = GetNextStartPositions(lawn, rectangle);

        if (pathstep.NextStartPositions.Count == 0)
        {
            // can not continue from here
            pathstep.IsValid = false;

            if (coveredArea > 0.95 * lawn.Fields)
            {
                pathstep.ChooseDifferentPath = true;
            }

        }
        else
        {

            // flood fill from next start position to check for unreachable parts
            if (lawn.PathSteps.Count > 3)
            {
                var reachableArea = GetReachableArea(lawn, pathstep.NextStartPositions.First());
                if (coveredArea + reachableArea < lawn.Fields)
                {
                    pathstep.IsValid = false;
                    return;
                }
            }

            var rectangles = new List<PathRectangle>();
            foreach (var startPos in pathstep.NextStartPositions)
            {
                rectangles.AddRange(FindRectangles(lawn, startPos));
            }
            pathstep.NextRectangles = rectangles.OrderByDescending(r => r.Area).ToList();
            pathstep.NextRectangleIndex = 0;


        }

    }

    private int GetReachableArea(Lawn lawn, Vector2 startposition)
    {
        var reachablePositions = new HashSet<Vector2>();
        reachablePositions.Add(startposition);

        var availablePositions = new Queue<Vector2>();
        availablePositions.Enqueue(startposition);

        while (availablePositions.Count > 0)
        {
            var testpos = availablePositions.Dequeue();

            foreach (var dir in MathUtils.OrthogonalDirections)
            {
                var newPos = testpos + dir;
                if (reachablePositions.Contains(newPos))
                {
                    continue;
                }
                if (IsValidStartPosition(lawn, newPos))
                {
                    reachablePositions.Add(newPos);
                    availablePositions.Enqueue(newPos);
                }
            }

        }

        return reachablePositions.Count;
    }

    private List<Vector2> GetNextStartPositions(Lawn lawn, PathRectangle rectangle)
    {
        var startPositions = new List<Vector2>();
        foreach (var dir in MathUtils.OrthogonalDirections)
        {
            var testPos = rectangle.EndPosition + dir;
            if (IsValidStartPosition(lawn, testPos))
            {
                startPositions.Add(testPos);
            }
        }
        return startPositions;
    }

    private bool IsValidStartPosition(Lawn lawn, Vector2 testPos)
    {
        // outside lawn
        if (testPos.X < 0 || testPos.X >= lawn.Width || testPos.Y < 0 || testPos.Y >= lawn.Height)
        {
            return false;
        }

        // inside pathing rectangle or tree
        foreach (var stuff in lawn.ObjectsOnLawn)
        {
            if (Intersect(testPos, stuff))
            {
                return false;
            }
        }

        return true;
    }

    private List<PathRectangle> FindRectangles(Lawn lawn, Vector2 startPosition)
    {
        var startX = (int)startPosition.X;
        var startY = (int)startPosition.Y;

        var rectangles = new List<PathRectangle>();

        var minX = 0;
        var minY = 0;
        var maxX = lawn.Width - 1;
        var maxY = lawn.Height - 1;

        // find objects in same row / column
        var objectsOnX = lawn.ObjectsOnLawn.Where(r => r.UpperLeftCornerY <= startY && r.LowerRightCornerY >= startY);
        var objectsOnY = lawn.ObjectsOnLawn.Where(r => r.UpperLeftCornerX <= startX && r.LowerRightCornerX >= startX);

        var objectsLeft = objectsOnX.Where(r => r.LowerRightCornerX < startX);
        var objectsRight = objectsOnX.Where(r => r.UpperLeftCornerX > startX);
        var objectsAbove = objectsOnY.Where(r => r.LowerRightCornerY < startY);
        var objectsBelow = objectsOnY.Where(r => r.UpperLeftCornerY > startY);


        // objects closest to start position in all 4 directions
        var obstacles = new HashSet<Rectangle>();

        if (objectsLeft.Count() > 0)
        {
            var border = objectsLeft.Max(r => r.LowerRightCornerX);
            var obstacle = objectsLeft.First(r => r.LowerRightCornerX == border);
            obstacles.Add(obstacle);
            minX = border + 1;
        }
        if (objectsRight.Count() > 0)
        {
            var border = objectsRight.Min(r => r.UpperLeftCornerX);
            var obstacle = objectsRight.First(r => r.UpperLeftCornerX == border);
            obstacles.Add(obstacle);
            maxX = border - 1;
        }
        if (objectsAbove.Count() > 0)
        {
            var border = objectsAbove.Max(r => r.LowerRightCornerY);
            var obstacle = objectsAbove.First(r => r.LowerRightCornerY == border);
            obstacles.Add(obstacle);
            minY = border + 1;
        }
        if (objectsBelow.Count() > 0)
        {
            var border = objectsBelow.Min(r => r.UpperLeftCornerY);
            var obstacle = objectsBelow.First(r => r.UpperLeftCornerY == border);
            obstacles.Add(obstacle);
            maxY = border - 1;
        }

        // rectangle spanning area between obstacles in row / column
        var borderRectangle = new Rectangle(new Vector2(minX, minY), new Vector2(maxX, maxY));

        // obstacles inside this area
        foreach (var objectOnLawn in lawn.ObjectsOnLawn)
        {
            if (Intersect(borderRectangle, objectOnLawn))
            {
                obstacles.Add(objectOnLawn);
            }
        }

        // edge positions of future test rectangles at edges of existing obstacles
        var edgeX = new HashSet<int>()
        {
            startX,
            minX,
            maxX
        };

        var edgeY = new HashSet<int>()
        {
            minY,
            startY,
            maxY
        };

        foreach (var obstacle in obstacles)
        {
            if (obstacle.UpperLeftCornerX > 0)
            {
                edgeX.Add(obstacle.UpperLeftCornerX - 1);
            }
            edgeX.Add(obstacle.UpperLeftCornerX);
            if (obstacle.LowerRightCornerX < lawn.Width - 1)
            {
                edgeX.Add(obstacle.LowerRightCornerX + 1);
            }

            if (obstacle.UpperLeftCornerY > 0)
            {
                edgeY.Add(obstacle.UpperLeftCornerY - 1);
            }
            edgeY.Add((int)obstacle.UpperLeftCornerY);
            if (obstacle.LowerRightCornerY < lawn.Height - 1)
            {
                edgeY.Add(obstacle.LowerRightCornerY + 1);
            }
        }

        // create rectangles from startposition to edges
        foreach (var xPosition in edgeX)
        {
            foreach (var yPosition in edgeY)
            {
                var rect = new PathRectangle(new Vector2(Math.Min(xPosition, startX), Math.Min(yPosition, startY)),
                                             new Vector2(Math.Max(xPosition, startX), Math.Max(yPosition, startY)))
                {
                    StartPosition = startPosition
                };


                // obstacles inside this rectangle, don't use
                var use = true;
                foreach (var objectOnLawn in lawn.ObjectsOnLawn)
                {
                    if (Intersect(rect, objectOnLawn))
                    {
                        use = false;
                        break;
                    }
                }
                if (!use)
                {
                    continue;
                }

                var xVector = xPosition - startX;
                var yVector = yPosition - startY;

                var xDir = xVector == 0 ? new Vector2(0, 0) : new Vector2(xVector / Math.Abs(xVector), 0);
                var yDir = yVector == 0 ? new Vector2(0, 0) : new Vector2(0, yVector / Math.Abs(yVector));

                if (xVector != 0 && yVector != 0)
                {
                    var rect2 = new PathRectangle(rect);

                    rect.FirstMoveDir = xDir;
                    rect.SecondMoveDir = yDir;

                    rect2.FirstMoveDir = yDir;
                    rect2.SecondMoveDir = xDir;

                    // try meandering in both directions
                    rect.GetEndPosition();
                    rect2.GetEndPosition();

                    rectangles.Add(rect);

                    if (rect2.EndPosition != rect.EndPosition)
                    {
                        rectangles.Add(rect2);
                    }
                }
                else if (xVector != 0)
                {
                    rect.FirstMoveDir = xDir;
                    rect.GetEndPosition();
                    rectangles.Add(rect);
                }
                else
                {
                    rect.FirstMoveDir = yDir;
                    rect.GetEndPosition();
                    rectangles.Add(rect);
                }

            }
        }

        // sort by area (try rectangles with larger area first)
        return rectangles.OrderByDescending(r => r.Area).ToList();
    }

    private bool Intersect(Rectangle rec1, Rectangle rec2)
    {
        return rec1.UpperLeftCornerX <= rec2.LowerRightCornerX &&
            rec1.LowerRightCornerX >= rec2.UpperLeftCornerX &&
            rec1.UpperLeftCornerY <= rec2.LowerRightCornerY &&
            rec1.LowerRightCornerY >= rec2.UpperLeftCornerY;
    }

    private bool Intersect(Vector2 position, Rectangle rectangle)
    {
        return position.X >= rectangle.UpperLeftCornerX &&
               position.X <= rectangle.LowerRightCornerX &&
               position.Y >= rectangle.UpperLeftCornerY &&
               position.Y <= rectangle.LowerRightCornerY;
    }

    public void CreatePathfromSteps(Lawn lawn)
    {
        var path = new List<Vector2>();
        foreach (var step in lawn.PathSteps)
        {
            step.CreatePath();
            path.AddRange(step.Path);
        }

        lawn.Path = path;
    }
}