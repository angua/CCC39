using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCC39Lib;

public class PathingStep
{
    private PathRectangle _rectangle;

    private List<PathingStep> _nextSteps = new();

    public PathingStep(PathRectangle rectangle)
    {
        _rectangle = rectangle;
    }
}
