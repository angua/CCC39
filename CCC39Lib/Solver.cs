using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using CCC39UI;
using Common;

namespace CCC39Lib;

public class Solver
{
    private static Vector2 _up = new Vector2(0, -1);
    private static Vector2 _left = new Vector2(-1, 0);
    private static Vector2 _right = new Vector2(1, 0);
    private static Vector2 _down = new Vector2(0, 1);

    public long Timing { get; private set; }
    public long TotalTiming { get; private set; }

    public string Solve(int level, List<string> lines)
    {
        return level switch
        {
            1 => SolveLevel1(lines),
            2 => SolveLevel2(lines),
            3 => SolveLevel3(lines),
            4 => SolveLevel4(4, lines),
            5 => SolveLevel4(5, lines),
            6 => SolveLevel4(6, lines, true),
            7 => SolveLevel4(7, lines, true),
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


    private string SolveLevel4(int level, List<string> lines, bool useCycle = false)
    {
        long totalTime = 0;
        var fullResult = new StringBuilder();

        var lawnSet = new LawnSet(level, lines);

        var lawnNum = 0;

        foreach (var lawn in lawnSet.Lawns)
        {
            FindPath(lawn, useCycle);
            fullResult.AppendLine(lawn.InstructionString);
            Console.WriteLine($"Lawn {lawnNum++}, took {Timing} ms");
            totalTime += Timing;
            lawn.ClearPath();
        }

        Console.WriteLine($"File took {totalTime} ms");
        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }


    public void FindPath(Lawn lawn, bool useCycle = false)
    {
        var watch = new Stopwatch();
        watch.Start();
        if (lawn.TreePositions.Count < 2)
        {
            // find path using rectangles
            while (!lawn.MowingFinished)
            {
                FindRectanglePathNextStep(lawn, useCycle);
            }
        }
        else
        {
            // find path by expanding from circle path
            while (!lawn.MowingFinished)
            {
                ExpandPathNextStep(lawn);
            }

        }
        watch.Stop();
        Timing = watch.ElapsedMilliseconds;
    }

    public void FindPathNextStep(Lawn lawn, bool useCycle = false)
    {
        if (lawn.TreePositions.Count < 2)
        {
            FindRectanglePathNextStep(lawn, useCycle);
        }
        else
        {
            ExpandPathNextStep(lawn);
        }
    }



    private void ExpandPathNextStep(Lawn lawn)
    {
        if (lawn.StartPathRevision == null)
        {
            lawn.StartPathRevision = new PathRevision();

            // create path encircling lawn, avoiding trees
            // start in upper left corner
            var currentPos = new Vector2(0, 0);

            // start by moving down
            var moveDir = _down;
            var currentMoveDir = _down;

            while (lawn.TreePositions.Contains(currentPos))
            {
                // move start position if there is a tree there
                currentPos += currentMoveDir;

                if (!lawn.InsideLawn(currentPos))
                {
                    currentPos = new Vector2(currentPos.X + 1, 0);
                }
            }
            // no tree
            lawn.StartPathRevision.Path.Add(currentPos);

            // walk along the edges of the lawn, finished when next to start position
            while (lawn.StartPathRevision.Path.Count < 2 * lawn.Height + 2 * lawn.Width - 4 ||
                (currentPos - lawn.StartPathRevision.Path.First()).Length() > 1)
            {
                var nextPos = currentPos + currentMoveDir;

                if (!lawn.InsideLawn(nextPos))
                {
                    if (currentMoveDir != moveDir)
                    {
                        // moved outside after surrounding tree
                        currentMoveDir = moveDir;
                        continue;
                    }

                    // turn left at the edges
                    moveDir = MathUtils.TurnLeft(moveDir);
                    currentMoveDir = moveDir;
                    continue;
                }

                // avoid tree
                if (lawn.TreePositions.Contains(nextPos))
                {
                    // move left towards the center of the lawn
                    currentMoveDir = MathUtils.TurnLeft(currentMoveDir);
                    continue;
                }

                // next position is valid
                currentPos = nextPos;
                lawn.StartPathRevision.Path.Add(currentPos);

                // not next to wall, try to return to the wall
                if (!lawn.IsEdgePosition(currentPos))
                {
                    currentMoveDir = MathUtils.TurnRight(currentMoveDir);
                }

            }

            lawn.PathRevisions.Add(lawn.StartPathRevision);
        }
        else
        {
            // find first suitable position for folding inwards
            var currentRevision = lawn.PathRevisions.Last();

            if (currentRevision.IsValid == false)
            {
                lawn.PathRevisions.RemoveAt(lawn.PathRevisions.Count - 1);
                var previousRevision = lawn.PathRevisions.Last();
                previousRevision.ForbiddenExpandPositions.Add(currentRevision.ExpandPosition);
                return;
            }

            var found = false;


            for (int i = 1; i < currentRevision.Path.Count - 1; i++)
            {
                var currentPos = currentRevision.Path[i];
                if (currentRevision.ForbiddenExpandPositions.Contains(currentPos))
                {
                    continue;
                }

                var nextPos = currentRevision.Path[i + 1];

                // direction from one path position to next
                var dir = nextPos - currentPos;

                // move left perpendicular to path from first position
                var left = MathUtils.TurnLeft(dir);
                var testpos1 = currentPos + left;

                if (lawn.TreePositions.Contains(testpos1))
                {
                    continue;
                }
                if (currentRevision.Path.Contains(testpos1))
                {
                    continue;
                }

                // position next to second position
                var testpos2 = nextPos + left;

                if (lawn.TreePositions.Contains(testpos2))
                {
                    continue;
                }
                if (currentRevision.Path.Contains(testpos2))
                {
                    continue;
                }

                // both positions are free, fold path inward
                var newPath = new List<Vector2>(currentRevision.Path);

                newPath.Insert(i + 1, testpos2);
                newPath.Insert(i + 1, testpos1);

                var newRevision = new PathRevision();
                newRevision.Path = newPath;
                newRevision.ExpandPosition = currentPos;

                // validity check
                // positions surrounding new path points

                if (!PerformValidityCheck(lawn, newRevision, testpos1) || !PerformValidityCheck(lawn, newRevision, testpos2))
                {
                    currentRevision.ForbiddenExpandPositions.Add(currentPos);
                    continue;
                }


                lawn.PathRevisions.Add(newRevision);
                found = true;
                if (newRevision.Path.Count == lawn.Fields)
                {
                    lawn.MowingFinished = true;
                    return;
                }


                break;

            }

            if (!found)
            {
                currentRevision.IsValid = false;
            }

        }


    }

    private bool PerformValidityCheck(Lawn lawn, PathRevision revision, Vector2 testpos)
    {
        for (int j = 0; j < 4; j++)
        {
            var neighbordir = MathUtils.OrthogonalDirections[j];
            var neighborpos = testpos + neighbordir;

            if (lawn.TreePositions.Contains(neighborpos))
            {
                continue;
            }
            if (revision.Path.Contains(neighborpos))
            {
                continue;
            }

            // empty neighbor position
            var emptyPositionsnext = 0;
            for (int k = 0; k < 4; k++)
            {
                var checkdir = MathUtils.OrthogonalDirections[k];
                var checkpos = neighborpos + checkdir;

                if (lawn.TreePositions.Contains(checkpos))
                {
                    continue;
                }
                if (revision.Path.Contains(checkpos))
                {
                    continue;
                }
                // empty position next to empty neighbor
                emptyPositionsnext++;
            }

            if (emptyPositionsnext == 0)
            {
                // single empty position, can not be filled, invalid
                revision.IsValid = false;
                return false;
            }
        }
        return true;

    }

    public void FindRectanglePathNextStep(Lawn lawn, bool useCycle = false)
    {
        lawn.PathStepsCount++;

        // start pathing
        if (lawn.StartPositions.Count == 0)
        {
            // create start positions
            lawn.SetStartPositions();
            lawn.StartPathStep = PathStep.CreateEmptyPathStep(lawn);
            if (lawn.TreePositions.Count > 1)
            {
                lawn.StartPathStep.NextStartPositions = new List<Vector2>()
                {
                    lawn.StartPositions.First()
                };
            }
            else
            {
                lawn.StartPathStep.NextStartPositions = lawn.StartPositions;
            }

            foreach (var pos in lawn.StartPathStep.NextStartPositions)
            {
                var rectangles = lawn.StartPathStep.FindRectangles(lawn, pos);

                foreach (var rectangle in rectangles)
                {
                    var pathstep = PathStep.CreatePathStep(lawn, null, rectangle, useCycle);
                    if (pathstep.IsValid)
                    {
                        lawn.StartPathStep.NextSteps.Add(pathstep);
                        pathstep.MowingStart = pathstep.PathingRectangle.StartPosition;
                    }
                }

            }
        }
        else
        {
            DoNextStep(lawn, useCycle);
        }

    }

    private void DoNextStep(Lawn lawn, bool useCycle = false)
    {
        if (lawn.MowingFinished)
        {
            return;
        }

        lawn.StartPathStep.ProcessNextStep(lawn, useCycle);

        if (lawn.StartPathStep.MowingFinished)
        {
            // we did it!
            lawn.MowingFinished = true;

            // parse correct steps
            var nextCorrectStep = lawn.StartPathStep.CorrectNextStep;

            while (nextCorrectStep != null)
            {
                lawn.CorrectPathSteps.Add(nextCorrectStep);
                nextCorrectStep = nextCorrectStep.CorrectNextStep;
            }

            CreatePathfromSteps(lawn);
            lawn.Instructions = CreateDirectionsFromPath(lawn.Path);
        }

    }


    public void CreatePathfromSteps(Lawn lawn)
    {
        var path = new List<Vector2>();

        if (lawn.CorrectPathSteps.Count > 0)
        {

            foreach (var step in lawn.CorrectPathSteps)
            {
                step.CreatePath();
                path.AddRange(step.Path);
            }
            lawn.Path = path;

        }

        else if (lawn.PathRevisions.Count > 0)
        {
            lawn.Path = lawn.PathRevisions.Last().Path;
        }


    }

    public List<char> CreateDirectionsFromPath(List<Vector2> path)
    {
        var list = new List<char>();
        for (int i = 1; i < path.Count; i++)
        {
            var dir = path[i] - path[i - 1];
            list.Add(GetInstruction(dir));
        }
        return list;
    }

    private char GetInstruction(Vector2 dir)
    {
        if (dir == _up)
        {
            return 'W';
        }
        if (dir == _down)
        {
            return 'S';
        }
        if (dir == _left)
        {
            return 'A';
        }
        if (dir == _right)
        {
            return 'D';
        }

        throw new InvalidOperationException($"unknown direction {dir}");

    }


    /// <summary>
    /// Solve for level 4 or 5 and given file, output elapsed time in ms
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<long> PerformanceTest(List<string> lines)
    {
        var times = new List<long>();

        var lawnSet = new LawnSet(4, lines);

        foreach (var lawn in lawnSet.Lawns)
        {
            FindPath(lawn);
            times.Add(Timing);
            lawn.ClearPath();
        }
        return times;
    }

    public long PerformanceTest(Lawn lawn)
    {
        FindPath(lawn);
        return Timing;
    }

    public void CreateAllPaths(Lawn lawn)
    {
        lawn.CreateAllPaths();
    }
}