﻿using System.Drawing;
using System.Linq;
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

    public PathStep? PreviousStep { get; set; }


    public Vector2 MowingStart { get; set; }

    public List<Rectangle> ObjectsOnLawn { get; set; } = new();

    public int CoveredArea { get; set; }

    public int PathStepCount { get; set; }

    public bool MowingFinished { get; set; } = false;

    public List<Vector2> Path => PathingRectangle.Path;

    public bool IsValid { get; set; } = true;

    internal void CreatePath()
    {
        PathingRectangle.CreatePath();
    }

    internal bool ProcessNextStep(Lawn lawn, bool useCycle = false)
    {
        if (NextSteps.Count == 0)
        {
            // step was just created, need to create next steps from here
            foreach (var rect in NextRectangles)
            {
                var nextStep = CreatePathStep(lawn, this, rect, useCycle);
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
                if (!nextStep.ProcessNextStep(lawn, useCycle))
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

    public static PathStep CreatePathStep(Lawn lawn, PathStep? previousPathStep, PathRectangle rectangle, bool useCycle = false)
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
            pathstep.PreviousStep = previousPathStep;
            pathstep.ObjectsOnLawn = new List<Rectangle>(previousPathStep.ObjectsOnLawn);
            pathstep.CoveredArea = previousPathStep.CoveredArea + rectangle.Area;
            pathstep.PathStepCount = previousPathStep.PathStepCount + 1;
            pathstep.MowingStart = previousPathStep.MowingStart;
        }
        pathstep.ObjectsOnLawn.Add(rectangle);

        if (pathstep.CoveredArea == lawn.Fields)
        {
            // mowing finished
            if (useCycle)
            {
                for (int i = 0; i < MathUtils.OrthogonalDirections.Count; i++)
                {
                    var pos = rectangle.EndPosition + MathUtils.OrthogonalDirections[i];
                    if (pos.Equals(pathstep.MowingStart))
                    {
                        // end next to start position
                        pathstep.MowingFinished = true;
                        pathstep.IsValid = true;
                        return pathstep;
                    }
                }
                // end not next to start
                pathstep.IsValid = false;
                return pathstep;
            }
            else
            {
                // mowing finished
                pathstep.MowingFinished = true;
                pathstep.IsValid = true;
                return pathstep;
            }
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
            if (lawn.TreePositions.Count > 1 && pathstep.PathStepCount >= 2)
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

        var edgePositions = new HashSet<Vector2>();

        var rectangles = new List<PathRectangle>();

        var minX = 0;
        var minY = 0;
        var maxX = lawn.Width - 1;
        var maxY = lawn.Height - 1;

        // find objects in same row / column
        var objectsOnX = ObjectsOnLawn.Where(r => r.TopY <= startY && r.BottomY >= startY);
        var objectsOnY = ObjectsOnLawn.Where(r => r.LeftX <= startX && r.RightX >= startX);

        var objectsLeft = objectsOnX.Where(r => r.RightX < startX);
        var objectsRight = objectsOnX.Where(r => r.LeftX > startX);
        var objectsAbove = objectsOnY.Where(r => r.BottomY < startY);
        var objectsBelow = objectsOnY.Where(r => r.TopY > startY);

        // objects closest to start position in all 4 directions
        var obstacles = new List<Rectangle>();

        if (objectsLeft.Any())
        {
            var orderedObjects = objectsLeft.OrderByDescending(r => r.RightX);
            var obstacle = orderedObjects.First();
            minX = obstacle.RightX + 1;

            edgePositions.Add(new Vector2(minX, obstacle.TopY - 1));
            edgePositions.Add(new Vector2(minX, obstacle.TopY));
            edgePositions.Add(new Vector2(minX, obstacle.BottomY));
            edgePositions.Add(new Vector2(minX, obstacle.BottomY + 1));
        }
        if (objectsRight.Any())
        {
            var orderedObjects = objectsRight.OrderBy(r => r.LeftX);
            var obstacle = orderedObjects.First();
            maxX = obstacle.LeftX - 1;

            edgePositions.Add(new Vector2(minX, obstacle.TopY - 1));
            edgePositions.Add(new Vector2(maxX, obstacle.TopY));
            edgePositions.Add(new Vector2(maxX, obstacle.BottomY));
            edgePositions.Add(new Vector2(minX, obstacle.BottomY + 1));
        }
        if (objectsAbove.Any())
        {
            var orderedObjects = objectsAbove.OrderByDescending(r => r.BottomY);
            var obstacle = orderedObjects.First();
            minY = obstacle.BottomY + 1;

            edgePositions.Add(new Vector2(obstacle.LeftX - 1, minY));
            edgePositions.Add(new Vector2(obstacle.LeftX, minY));
            edgePositions.Add(new Vector2(obstacle.RightX, minY));
            edgePositions.Add(new Vector2(obstacle.RightX + 1, minY));
        }
        if (objectsBelow.Any())
        {
            var orderedObjects = objectsBelow.OrderBy(r => r.TopY);
            var obstacle = orderedObjects.First();
            maxY = obstacle.TopY - 1;

            edgePositions.Add(new Vector2(obstacle.LeftX - 1, maxY));
            edgePositions.Add(new Vector2(obstacle.LeftX, maxY));
            edgePositions.Add(new Vector2(obstacle.RightX, maxY));
            edgePositions.Add(new Vector2(obstacle.RightX + 1, maxY));
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

            // positions around corners of obstacle
            edgePositions.Add(new Vector2(obstacle.LeftX, obstacle.TopY - 1));
            edgePositions.Add(new Vector2(obstacle.LeftX - 1, obstacle.TopY - 1));
            edgePositions.Add(new Vector2(obstacle.LeftX - 1, obstacle.TopY));

            edgePositions.Add(new Vector2(obstacle.RightX, obstacle.TopY - 1));
            edgePositions.Add(new Vector2(obstacle.RightX + 1, obstacle.TopY - 1));
            edgePositions.Add(new Vector2(obstacle.RightX + 1, obstacle.TopY));

            edgePositions.Add(new Vector2(obstacle.LeftX, obstacle.BottomY + 1));
            edgePositions.Add(new Vector2(obstacle.LeftX - 1, obstacle.BottomY + 1));
            edgePositions.Add(new Vector2(obstacle.LeftX - 1, obstacle.BottomY));

            edgePositions.Add(new Vector2(obstacle.RightX, obstacle.BottomY + 1));
            edgePositions.Add(new Vector2(obstacle.RightX + 1, obstacle.BottomY + 1));
            edgePositions.Add(new Vector2(obstacle.RightX + 1, obstacle.BottomY));

            // direct line between start position and obstacle
            edgePositions.Add(new Vector2(startPosition.X, obstacle.TopY - 1));
            edgePositions.Add(new Vector2(startPosition.X, obstacle.BottomY + 1));
            edgePositions.Add(new Vector2(obstacle.LeftX - 1, startPosition.Y));
            edgePositions.Add(new Vector2(obstacle.RightX + 1, startPosition.Y));


        }

        // create rectangles from startposition to edges
        foreach (var xPosition in edgeX)
        {
            foreach (var yPosition in edgeY)
            {
                edgePositions.Add(new Vector2(xPosition, yPosition));
            }
        }

        foreach (var pos in edgePositions)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= lawn.Width || pos.Y >= lawn.Height)
            {
                // outside lawn
                continue;
            }

            var rect = new PathRectangle(startPosition, pos)
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

            var xVector = pos.X - startX;
            var yVector = pos.Y - startY;

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

    internal List<PathStep> GetLastSteps()
    {
        var lastSteps = new List<PathStep>();

        if (NextSteps.Count == 0)
        {
            lastSteps.Add(this);
        }
        else
        {
            foreach (var step in NextSteps)
            {
                lastSteps.AddRange(step.GetLastSteps());
            }
        }

        return lastSteps;
    }
}
