using System;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Random
{
    public class BellWeightedRandom : IRandom
    {
        private readonly System.Random _rand = new System.Random();
        private const double Mean = 0.5;
        private readonly Func<double, double, double> _genericBellFunc = (x, stdDev) => 1/((1 / (stdDev * Math.Sqrt(2 * Math.PI))) * Math.Pow(Math.E, (x - Mean) * (x - Mean) / (2 * stdDev * stdDev)));
        private readonly Func<double, double> _specificBellFunc;
        private readonly double _maxYOfBellCurve;

        public BellWeightedRandom(double stdDev)
        {
            _specificBellFunc = x => _genericBellFunc(x, stdDev);
            _maxYOfBellCurve = _specificBellFunc(Mean);
/*
            Console.WriteLine($"max of bell = {_maxYOfBellCurve}");
            for (double x = 0.0; x < 1; x+=0.01)
            {
                Console.WriteLine($"{x},{_specificBellFunc(x)}");
            }
*/
        }

        public double NextDouble()
        {
            while (true)
            {
                var x = _rand.NextDouble();
                var y = _rand.NextDouble() * _maxYOfBellCurve;

//                Console.WriteLine($"x = {x}, y = {y}, isUnder = {IsUnderCurve(x,y,_specificBellFunc)}");

                if (IsUnderCurve(x, y, _specificBellFunc))
    
                {
                    return x;
                }
            }
        }

        public double NextDouble(double minX, double maxX)
        {
            return minX + NextDouble() * (maxX - minX);
        }

        private bool IsUnderCurve(double x, double y, Func<double, double> func)
        {
            return y <= func(x);
        }
    }
}