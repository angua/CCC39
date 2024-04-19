using System.Text;

namespace CCC39Lib;

public class Solver
{
    public string Solve(int level, List<string> lines)
    {
        return level switch
        {
            1 => SolveLevel1(lines),
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
}