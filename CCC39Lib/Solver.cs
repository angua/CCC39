using System.Numerics;
using System.Text;

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
}