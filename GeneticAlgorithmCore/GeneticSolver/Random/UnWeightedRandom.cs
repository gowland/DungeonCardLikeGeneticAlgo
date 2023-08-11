using System.ComponentModel.Design;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Random
{
    public class UnWeightedRandom : IRandom
    {
        private readonly System.Random _rand = new System.Random();

        public double NextDouble()
        {
            return _rand.NextDouble();
        }

        public double NextDouble(double minX, double maxX)
        {
            return minX + NextDouble() * (maxX - minX);
        }

        public bool NextBool()
        {
            return NextDouble() > 0.5;
        }
    }
}