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
#if false
        lines = @"3
WASAWWDDDSS
DDSAASDDSAA
DSASSDWDSDWWAWDDDSASSDWDSDWWAWD".Split('\n').ToList();
#endif

        var actualLines = lines.Skip(1);
        var fullResult = new StringBuilder();

        foreach (var line in actualLines)
        {
            var currentPos = new Vector2(0, 0);

            var minX = 0;
            var minY = 0;
            var maxX = 0;
            var maxY = 0;

            foreach (var ch in line)
            {
                switch (ch)
                {
                    case 'W': currentPos += new Vector2(0, 1); break;
                    case 'A': currentPos += new Vector2(-1, 0); break;
                    case 'S': currentPos += new Vector2(0, -1); break;
                    case 'D': currentPos += new Vector2(1, 0); break;
                };


                minX = Math.Min((int)currentPos.X, minX);
                minY = Math.Min((int)currentPos.Y, minY);
                maxX = Math.Max((int)currentPos.X, maxX);
                maxY = Math.Max((int)currentPos.Y, maxY);
            }

            var resultLine = string.Join(' ', new[] { maxX - minX + 1, maxY - minY + 1 });
            fullResult.AppendLine(resultLine);
        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }
}