using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CCC39Lib
{
    internal class Route
    {
        public List<Vector2> Positions { get; set; } = new();

        public int Length => Positions.Count;
    }
}
