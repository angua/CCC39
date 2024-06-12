using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CCC39Lib;

internal class Lawn
{
    public int Width { get; set; }
    public int Height { get; set; }

    public List<Vector2> TreePositions { get; set; } = new();

}