﻿using System.Numerics;

namespace CCC39Lib;

public class Lawn
{
    public int Width { get; set; }
    public int Height { get; set; }

    // non-tree cells
    public int Fields => Width * Height - TreePositions.Count;

    public List<Vector2> TreePositions { get; set; } = new();

    // 1x1 rectangles for obstacle detection
    public HashSet<Rectangle> TreeRectangles { get; set; } = new();


    // series of xy positions on the lawn
    public List<Vector2> Path { get; set; } = new();

    // movement instructions
    public List<char> Instructions { get; set; } = new();

    public string InstructionString => string.Join("", Instructions);

    // start positions for path search
    public List<Vector2> StartPositions { get; set; } = new();
    public int StartPositionIndex { get; set; }
    public List<PathRectangle> NextPathrectangles { get; set; } = new();
    public int NextRectangleIndex { get; set; } = 0;

    public HashSet<PathRectangle> PathRectangles { get; set; } = new();
    public List<PathStep> PathSteps { get; set; } = new();

    // trees and path rectangles
    public HashSet<Rectangle> ObjectsOnLawn { get; set; } = new();

    public bool MowingFinished { get; set; } = false;

    public void ClearPath()
    {
        StartPositions.Clear();
        StartPositionIndex = 0;
        NextPathrectangles.Clear();
        NextRectangleIndex = 0;
        PathRectangles.Clear();
        PathSteps.Clear();
        ObjectsOnLawn = new HashSet<Rectangle>(TreeRectangles);
        MowingFinished = false;
    }

    public HashSet<Rectangle> GetObjectsOnLawn()
    {
        // trees and path rectangles

        var objectsOnLawn = new HashSet<Rectangle>(TreeRectangles);
        objectsOnLawn.UnionWith(PathRectangles.ToHashSet());
        return objectsOnLawn;
    }

    internal int GetCoveredArea()
    {
        return PathRectangles.Sum(r => r.Area);
    }


    public int ParseLawnMap(List<string> lines, int i)
    {
        var line = lines[i];

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        Width = Convert.ToInt32(parts[0]);
        Height = Convert.ToInt32(parts[1]);

        for (int y = 0; y < Height; y++)
        {
            // parse tree positions
            i++;
            line = lines[i];

            for (int x = 0; x < Width; x++)
            {
                if (line[x] == 'X')
                {
                    TreePositions.Add(new Vector2(x, y));
                }
            }
        }

        foreach (var pos in TreePositions)
        {
            TreeRectangles.Add(new Rectangle(pos, pos));
        }

        ObjectsOnLawn = new HashSet<Rectangle>(TreeRectangles);

        return i;
    }



    /*
     * Start positions surrounding the tree, on the edges at tree height and in the corners
        O..OOO.O
        ........
        O..OOO.O
        O..OXO.O
        O..OOO.O
        ........
        O..OOO.O
     */
    internal void SetStartPositions()
    {
        var treePos = TreePositions.First();

        var startX = new HashSet<int>();
        var startY = new HashSet<int>();

        if (treePos.X > 0)
        {
            startX.Add(((int)treePos.X - 1));
        }
        startX.Add((int)treePos.X);
        if (treePos.X < Width - 1)
        {
            startX.Add(((int)treePos.X + 1));
        }

        startX.Add(0);
        startX.Add(Width - 1);

        startY.Add(0);
        startY.Add(Height - 1);

        if (treePos.Y > 0)
        {
            startY.Add(((int)treePos.Y - 1));
        }
        startY.Add((int)treePos.Y);
        if (treePos.Y < Height - 1)
        {
            startY.Add(((int)treePos.Y + 1));
        }

        for (int xCoordinate = 0; xCoordinate < startX.Count; xCoordinate++)
        {
            for (int yCoordinate = 0; yCoordinate < startY.Count; yCoordinate++)
            {
                var pos = new Vector2(xCoordinate, yCoordinate);

                if (!TreePositions.Contains(pos))
                {
                    StartPositions.Add(pos);
                }
            }
        }

    }


}