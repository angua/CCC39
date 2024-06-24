using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AoC2023Lib.Days.Day10Lib;

namespace Common;

public class MathUtils
{
    public static List<List<int>> GetAllCombinations(int allElements, int selectedElements)
    {
        var allCombinations = new List<List<int>>();
        GetCombinations(allElements, selectedElements, new List<int>(), allCombinations);
        return allCombinations;
    }

    private static void GetCombinations(int positions, int remainingelements, List<int> previousplacements, List<List<int>> allCombinations)
    {
        if (remainingelements == 0)
        {
            allCombinations.Add(previousplacements);
            return;
        }
        var start = 0;
        if (previousplacements.Any())
        {
            start = previousplacements.Last() + 1;
        }

        var end = positions - remainingelements + 1;

        for (int i = start; i < end; i++)
        {
            var newplacements = new List<int>(previousplacements);
            newplacements.Add(i);
            GetCombinations(positions, remainingelements - 1, newplacements, allCombinations);
        }
    }

    /// <summary>
    /// Find the elements inside the edges
    /// </summary>
    /// <param name="edges">List of consecutive positions defining the edge of the area</param>
    /// <returns>List of positions inside the area</returns>
    public static HashSet<Vector2> GetInside(List<Vector2> edges)
    {
        var minX = edges.Min(p => p.X);
        var maxX = edges.Max(p => p.X);
        var minY = edges.Min(p => p.Y);
        var maxY = edges.Max(p => p.Y);

        var minBoundary = new Vector2(minX, minY);
        var maxBoundary = new Vector2(maxX, maxY);

        var outside = Side.Unknown;

        var rightPositions = new HashSet<Vector2>();
        var leftPositions = new HashSet<Vector2>();

        var hashEdges = edges.ToHashSet();

        for (int i = 0; i < edges.Count; i++)
        {
            var currentPosition = edges[i];
            var nextPosition = (i + 1 >= edges.Count) ? edges[0] : edges[i + 1];

            // move direction to next edge element
            var dir = nextPosition - currentPosition;

            // test positions orthogonal to current tile
            outside = TestOrthogonalPositions(currentPosition, hashEdges, dir, minBoundary, maxBoundary,
                                            rightPositions, leftPositions, outside);

            // test positions orthogonal to next tile
            outside = TestOrthogonalPositions(nextPosition, hashEdges, dir, minBoundary, maxBoundary,
                                                rightPositions, leftPositions, outside);
        }

        if (outside != Side.Right)
        {
            return rightPositions;
        }
        else
        {
            return leftPositions;
        }
    }


    private static Side TestOrthogonalPositions(Vector2 currentPosition, HashSet<Vector2> edges, Vector2 dir,
                                            Vector2 minBoundary, Vector2 maxBoundary,
                                            HashSet<Vector2> rightPositions, HashSet<Vector2> leftPositions, Side outside)
    {
        // test directions are orthogonal to move direction
        var rightDir = TurnRight(dir);

        // don't collect new positions on outside
        if (outside == Side.Left || outside == Side.Unknown)
        {
            // positions right of current position
            var rightTestPos = currentPosition + rightDir;

            // add new right positions to hashset
             FindPositions(rightPositions, rightTestPos, edges, minBoundary, maxBoundary, out var rightIsOutside);
            if (rightIsOutside)
            {
                outside = Side.Right;
            }
        }

        if (outside == Side.Right || outside == Side.Unknown)
        {
            var leftTestPos = currentPosition - rightDir;
            // add new left positons
            FindPositions(leftPositions, leftTestPos, edges, minBoundary, maxBoundary, out var leftIsOutside);
            if (leftIsOutside)
            {
                outside = Side.Left;
            }
        }
        return outside;
    }

    private static void FindPositions(HashSet<Vector2> foundPositions, Vector2 startPos, HashSet<Vector2> edges,
                                    Vector2 minBoundary, Vector2 maxBoundary, out bool isOutside)
    {
        isOutside = false;

        // already found, no need to start finding from here
        if (foundPositions.Contains(startPos))
        {
            return;
        }

        // flood fill
        var availablePositions = new Stack<Vector2>();
        var visitedPositions = new HashSet<Vector2>();

        availablePositions.Push(startPos);

        while (availablePositions.Count > 0)
        {
            var testPos = availablePositions.Pop();

            if (InsideBoundary(testPos, minBoundary, maxBoundary))
            {
                if (!edges.Contains(testPos))
                {
                    // this is not part of the loop, add to list
                    foundPositions.Add(testPos);

                    foreach (var dir in OrthogonalDirections)
                    {
                        var newPos = testPos + dir;
                        if (!visitedPositions.Contains(newPos))
                        {
                            availablePositions.Push(newPos);
                        }
                    }
                }
            }
            else
            {
                // outside boundary, reached outside
                isOutside = true;
                break;
            }
            visitedPositions.Add(testPos);
        }

    }

    public static bool InsideBoundary(Vector2 testPos, Vector2 minBoundary, Vector2 maxBoundary)
    {
        return (testPos.X >= minBoundary.X &&
                testPos.X <= maxBoundary.X &&
                testPos.Y >= minBoundary.Y &&
                testPos.Y <= maxBoundary.Y);
    }

    public static Vector2 TurnRight(Vector2 input)
    {
        return new Vector2(-input.Y, input.X);
    }
    public static Vector2 TurnLeft(Vector2 input)
    {
        return new Vector2(input.Y, -input.X);
    }

    public static List<Vector2> OrthogonalDirections => new List<Vector2>()
    {
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(-1, 0),
        new Vector2(0, -1)
    };

    /// <summary>
    /// Return the rasterized line from start to end, including the start / end points themselves.
    /// </summary>
    public static IEnumerable<Vector2> GetPointsOnLine(Vector2 start, Vector2 end)
    {
        var delta = end - start;

        if (Math.Abs(delta.X) > Math.Abs(delta.Y))
        {
            var numSteps = Math.Abs(delta.X);
            var deltaX = delta.X < 1 ? -1 : 1;
            for (var i = 0; i < numSteps; ++i)
            {
                var yStep = delta.Y / numSteps;
                yield return new Vector2(start.X + deltaX * i, (int)(start.Y + yStep * i));
            }
        }
        else
        {
            var numSteps = Math.Abs(delta.Y);
            var deltaY = delta.Y < 1 ? -1 : 1;
            for (var i = 0; i < numSteps; ++i)
            {
                var xStep = delta.X / numSteps;
                yield return new Vector2((int)(start.X + xStep * i), start.Y + deltaY * i);
            }
        }

        yield return end;
    }

}
