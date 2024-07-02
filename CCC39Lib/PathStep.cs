using System.Drawing;
using System.Numerics;
using Common;

namespace CCC39Lib;

public class PathStep
{
    public PathRectangle PathingRectangle { get; set; }

    public List<Vector2> NextStartPositions { get; set; }
    public List<PathRectangle> NextRectangles { get; set; }

    public List<PathStep> NextSteps { get; set; } = new();

    public PathStep? CorrectNextStep { get; set; }


    public List<Rectangle> ObjectsOnLawn { get; set; } = new();

    public int CoveredArea { get; set; }

    public int PathStepCount { get; set; }

    public bool MowingFinished { get; set; } = false;

    public int NextRectangleIndex { get; set; }

    public List<Vector2> Path => PathingRectangle.Path;

    public bool IsValid { get; set; } = true;

    public bool ChooseDifferentPath { get; set; } = false;

    internal void CreatePath()
    {
        PathingRectangle.CreatePath();
    }

    internal bool ProcessNextStep(Lawn lawn)
    {
        if (NextSteps.Count == 0)
        {
            // step was just created, need to create next steps from here
            foreach (var rect in NextRectangles)
            {
                var nextStep = CreatePathStep(lawn, this, rect);
                if (nextStep.MowingFinished)
                {
                    MowingFinished = true;
                    CorrectNextStep = nextStep;
                    return true;
                }
                if (nextStep.IsValid)
                {
                    NextSteps.Add(nextStep);
                }
            }
            if (NextSteps.Count == 0)
            {
                // no valid steps from here
                return false;
            }
        }
        else
        {
            var stepsToBeRemoved = new List<PathStep>();
            foreach (var nextStep in NextSteps)
            {
                if (!nextStep.ProcessNextStep(lawn))
                {
                    stepsToBeRemoved.Add(nextStep);
                }
                else if (nextStep.MowingFinished)
                {
                    MowingFinished = true;
                    CorrectNextStep = nextStep;
                    return true;
                }
            }
            foreach (var invalidStep in stepsToBeRemoved)
            {
                NextSteps.Remove(invalidStep);
            }
            if (NextSteps.Count == 0)
            {
                // no more valid steps from here
                return false;
            }
        }
        return true;
    }

    public static PathStep CreatePathStep(Lawn lawn, PathStep? previousPathStep, PathRectangle rectangle)
    {
        var pathstep = new PathStep()
        {
            PathingRectangle = rectangle,
        };

        if (previousPathStep == null)
        {
            pathstep.ObjectsOnLawn = new List<Rectangle>(lawn.TreeRectangles);
            pathstep.CoveredArea = rectangle.Area;
            pathstep.PathStepCount = 1;
        }
        else
        {
            pathstep.ObjectsOnLawn = new List<Rectangle>(previousPathStep.ObjectsOnLawn);
            pathstep.CoveredArea = previousPathStep.CoveredArea + rectangle.Area;
            pathstep.PathStepCount = previousPathStep.PathStepCount + 1;
        }
        pathstep.ObjectsOnLawn.Add(rectangle);

        if (pathstep.CoveredArea == lawn.Fields)
        {
            // mowing finished
            pathstep.MowingFinished = true;
            pathstep.IsValid = true;
            return pathstep;
        }

        pathstep.NextStartPositions = pathstep.GetNextStartPositions(lawn, rectangle);

        if (pathstep.NextStartPositions.Count == 0)
        {
            // can not continue from here
            pathstep.IsValid = false;
            return pathstep;
        }
        else
        {
            // flood fill from next start position to check for unreachable parts
            if (false)
            {
                var reachableArea = pathstep.GetReachableArea(lawn, pathstep.NextStartPositions.First());
                if (pathstep.CoveredArea + reachableArea < lawn.Fields)
                {
                    pathstep.IsValid = false;
                    return pathstep;
                }
            }

            var rectangles = new List<PathRectangle>();
            foreach (var startPos in pathstep.NextStartPositions)
            {
                rectangles.AddRange(pathstep.FindRectangles(lawn, startPos));
            }
            pathstep.NextRectangles = rectangles.OrderByDescending(r => r.Area).ToList();

            pathstep.NextRectangleIndex = 0;
        }
        return pathstep;
    }

    /// <summary>
    /// Create path step without rectangle
    /// </summary>
    /// <param name="lawn">The Lawn</param>
    /// <returns></returns>
    public static PathStep CreateEmptyPathStep(Lawn lawn)
    {
        var pathstep = new PathStep();
        pathstep.ObjectsOnLawn = new List<Rectangle>(lawn.TreeRectangles);
        pathstep.CoveredArea = 0;
        pathstep.PathStepCount = 0;

        return pathstep;
    }


    public List<PathRectangle> FindRectangles(Lawn lawn, Vector2 startPosition)
    {
        var startX = (int)startPosition.X;
        var startY = (int)startPosition.Y;

        var rectangles = new List<PathRectangle>();

        var minX = 0;
        var minY = 0;
        var maxX = lawn.Width - 1;
        var maxY = lawn.Height - 1;

        // find objects in same row / column
        var objectsOnX = ObjectsOnLawn.Where(r => r.TopY <= startY && r.BottomY >= startY).ToList();
        var objectsOnY = ObjectsOnLawn.Where(r => r.LeftX <= startX && r.RightX >= startX).ToList();
                                                                                                                
        var objectsLeft = objectsOnX.Where(r => r.RightX < startX).ToList();
        var objectsRight = objectsOnX.Where(r => r.LeftX > startX).ToList();
        var objectsAbove = objectsOnY.Where(r => r.BottomY < startY).ToList();
        var objectsBelow = objectsOnY.Where(r => r.TopY > startY).ToList();


        // objects closest to start position in all 4 directions
        var obstacles = new List<Rectangle>();

        if (objectsLeft.Count() > 0)
        {
            var orderedObjects = objectsLeft.OrderByDescending(r => r.RightX);
            var obstacle = orderedObjects.First();
            var border = obstacle.RightX;
            obstacles.Add(obstacle);
            minX = border + 1;
        }
        if (objectsRight.Count() > 0)
        {
            var orderedObjects = objectsRight.OrderBy(r => r.LeftX);
            var obstacle = orderedObjects.First();
            var border = obstacle.LeftX;

            obstacles.Add(obstacle);
            maxX = border - 1;
        }
        if (objectsAbove.Count() > 0)
        {
            var orderedObjects = objectsAbove.OrderByDescending(r => r.BottomY);
            var obstacle = orderedObjects.First();
            var border = obstacle.BottomY;

            obstacles.Add(obstacle);
            minY = border + 1;
        }
        if (objectsBelow.Count() > 0)
        {
            var orderedObjects = objectsBelow.OrderBy(r => r.TopY);
            var obstacle = orderedObjects.First();
            var border = obstacle.TopY;

            obstacles.Add(obstacle);
            maxY = border - 1;
        }

        // rectangle spanning area between obstacles in row / column
        var borderRectangle = new Rectangle(minX, maxX, minY, maxY);

        // obstacles inside this area
        for (int i = 0; i < ObjectsOnLawn.Count; i++)
        {
            var objectOnLawn = ObjectsOnLawn[i];
            if (borderRectangle.Intersect(objectOnLawn))
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

        for (int i = 0; i < obstacles.Count; i++)
        {
            var obstacle = obstacles[i];
            if (obstacle.LeftX > 0)
            {
                edgeX.Add(obstacle.LeftX - 1);
            }
            edgeX.Add(obstacle.LeftX);
            if (obstacle.RightX < lawn.Width - 1)
            {
                edgeX.Add(obstacle.RightX + 1);
            }

            if (obstacle.TopY > 0)
            {
                edgeY.Add(obstacle.TopY - 1);
            }
            edgeY.Add((int)obstacle.TopY);
            if (obstacle.BottomY < lawn.Height - 1)
            {
                edgeY.Add(obstacle.BottomY + 1);
            }
        }

        // create rectangles from startposition to edges
        foreach (var xPosition in edgeX)
        {
            foreach (var yPosition in edgeY)
            {
                var leftX = Math.Min(xPosition, startX);
                var rightX = Math.Max(xPosition, startX);
                var topY = Math.Min(yPosition, startY);
                var bottomY = Math.Max(yPosition, startY);

                var rect = new PathRectangle(leftX, rightX, topY, bottomY)
                {
                    StartPosition = startPosition
                };

                // obstacles inside this rectangle, don't use
                var use = true;
                for (int i = 0; i < ObjectsOnLawn.Count; i++)
                {
                    var objectOnLawn = ObjectsOnLawn[i];
                    if (rect.Intersect(objectOnLawn))
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


    public List<Vector2> GetNextStartPositions(Lawn lawn, PathRectangle rectangle)
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

    public int GetReachableArea(Lawn lawn, Vector2 startposition)
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

    private bool IsValidStartPosition(Lawn lawn, Vector2 testPos)
    {
        // outside lawn
        if (testPos.X < 0 || testPos.X >= lawn.Width || testPos.Y < 0 || testPos.Y >= lawn.Height)
        {
            return false;
        }

        // inside pathing rectangle or tree
        foreach (var rect in ObjectsOnLawn)
        {
            if (rect.Intersect(testPos))
            {
                return false;
            }
        }

        return true;
    }

}
