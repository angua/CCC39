using System.IO;
using System.Numerics;
using CCC39Lib;

namespace CCC39UI;

public class LawnSet
{

    public LawnSet(int level, int stage)
    {
        InputFilename = $"../../../../Files/level{level}_{stage}.in";
        OutputFilename = Path.ChangeExtension(InputFilename, ".out");
        Lines = File.ReadAllLines(InputFilename).ToList();

        Lawns = ParseLawns(level, Lines);

        Level = level;
    }

    public LawnSet(int level, List<string> lines)
    {
        Lines = lines;
        Lawns = ParseLawns(level, lines);

        Level = level;
    }

    public int NumLawns { get; private set; }

    public int Level { get; }

    public string? InputFilename { get; }
    public string? OutputFilename { get; }
    public List<string> Lines { get; }

    public List<Lawn> Lawns { get; } = new List<Lawn>();

    private List<Lawn> ParseLawns(int level, List<string> lines)
    {
        switch (level)
        {
            case 1:
                return ParseInstructions(lines);
            case 2:
                return ParseInstructions(lines);

            case 3:
                return ParseMapAndInstructions(lines);

            case 4:
                return ParseLawnsFromMaps(lines);
            case 5:
                return ParseLawnsFromMaps(lines);
            case 6:
                return ParseLawnsFromMaps(lines);


            default:
                return new List<Lawn>();
        }
    }

    private List<Lawn> ParseMapAndInstructions(List<string> lines)
    {
        NumLawns = Convert.ToInt32(lines.First());

        var lawns = new List<Lawn>();

        var i = 1;
        while (i < lines.Count)
        {
            var lawn = new Lawn();

            i = lawn.ParseLawnMap(lines, i);
            i++;
            var line = lines[i];
            SetInstructions(lawn, line);

            lawns.Add(lawn);

            // next lawn
            i++;
        }
        return lawns;

    }

    private List<Lawn> ParseInstructions(List<string> lines)
    {
        NumLawns = Convert.ToInt32(lines.First());

        var lawns = new List<Lawn>();

        var i = 1;
        while (i < lines.Count)
        {
            // new lawn
            var lawn = new Lawn();

            // SDDDDWWWWWWWAASDSA...
            var line = lines[i];
            SetInstructions(lawn, line);

            var maxX = lawn.Path.Max(p => p.X);
            var maxY = lawn.Path.Max(p => p.Y);

            lawn.Width = (int)maxX + 1;
            lawn.Height = (int)maxY + 1;

            lawns.Add(lawn);

            i++;

        }
        return lawns;
    }

    private void SetInstructions(Lawn lawn, string line)
    {
        // path relative to starting point
        var relativePath = new List<Vector2>();
        var currentPos = new Vector2(0, 0);
        relativePath.Add(currentPos);

        foreach (var ch in line)
        {
            lawn.Instructions.Add(ch);

            var dir = GetDirection(ch);
            currentPos += dir;

            relativePath.Add(currentPos);
        }

        // move coordinate system to upper left corner
        var minX = relativePath.Min(p => p.X);
        var minY = relativePath.Min(p => p.Y);

        var upperLeftCorner = new Vector2(minX, minY);

        lawn.Path = relativePath.Select(p => p - upperLeftCorner).ToList();
    }

    private List<Lawn> ParseLawnsFromMaps(List<string> lines)
    {
        NumLawns = Convert.ToInt32(lines.First());

        var lawns = new List<Lawn>();

        var i = 1;
        while (i < lines.Count)
        {
            // new lawn
            var lawn = new Lawn();
            i = lawn.ParseLawnMap(lines, i);
            lawns.Add(lawn);

            // next lawn
            i++;
        }

        return lawns;
    }


    private Vector2 GetDirection(char ch)
    {
        return ch switch
        {
            'W' => new Vector2(0, -1),
            'A' => new Vector2(-1, 0),
            'S' => new Vector2(0, 1),
            'D' => new Vector2(1, 0),
            _ => throw new InvalidOperationException($"Invali´d movement instruction {ch}")
        };
    }
}


