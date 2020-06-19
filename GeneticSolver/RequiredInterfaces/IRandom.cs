using System.Collections.Generic;

namespace GeneticSolver.RequiredInterfaces
{
    public interface IRandom
    {
        double NextDouble();
        double NextDouble(double minX, double maxX);
        bool NextBool();
    }
}