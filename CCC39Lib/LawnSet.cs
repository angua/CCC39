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



    public int Level { get; }

    public string InputFilename { get; }
    public string OutputFilename { get; }
    public List<string> Lines { get; }

    public List<Lawn> Lawns { get; } = new List<Lawn>();

    private List<Lawn> ParseLawns(int level, List<string> lines)
    {
        switch (level)
        {
            

            case 4:
                return ParseLawnsFromMap(lines);

            default:
                return new List<Lawn>();
        }
    }

    private List<Lawn> ParseLawnsFromMap(List<string> lines)
    {
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



            // next lawn
            i++;
        }

        return lawns;
    }
}


