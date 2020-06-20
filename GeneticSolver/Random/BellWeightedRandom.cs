using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Expressions;
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

        public bool NextBool()
        {
            return NextDouble() > 0.5;
        }

        private bool IsUnderCurve(double x, double y, Func<double, double> func)
        {
            return y <= func(x);
        }
    }

    public class CyclableBellWeightedRandom : IRandom
    {
        private static readonly double[] StdDeviationsCycle = {0.01, 0.1, 0.2, 1};
        private Queue<IRandom> _currentQueue;
        private Queue<IRandom> _usedValueQueue = new Queue<IRandom>();

        public CyclableBellWeightedRandom(params double[] stdevs)
        {
            _currentQueue = new Queue<IRandom>((stdevs.Any() ? stdevs : StdDeviationsCycle).Select(stdev => new BellWeightedRandom(stdev)));
            _currentQueue.Enqueue(new UnWeightedRandom());
            CycleStdDev();
        }

        public double NextDouble()
        {
            return _usedValueQueue.Peek().NextDouble();
        }

        public double NextDouble(double minX, double maxX)
        {
            return _usedValueQueue.Peek().NextDouble(minX, maxX);
        }

        public bool NextBool()
        {
            return NextDouble() > 0.5;
        }

        public void CycleStdDev()
        {
            if (_currentQueue.Count <= 0)
            {
                var tmp = _currentQueue;
                _currentQueue = _usedValueQueue;
                _usedValueQueue = tmp;
            }

            IRandom currentStdDev = _currentQueue.Dequeue();
            _usedValueQueue.Enqueue(currentStdDev);
        }
    }

    public class Cyclable<T> : IValueSource<T>
    {
        private Queue<T> _currentQueue;
        private Queue<T> _usedValueQueue = new Queue<T>();

        public Cyclable(IEnumerable<T> values)
        {
            _currentQueue = new Queue<T>(values);
            Cycle();
        }

        public T CurrentValue => _usedValueQueue.Peek();

        public void Cycle()
        {
            if (_currentQueue.Count <= 0)
            {
                var tmp = _currentQueue;
                _currentQueue = _usedValueQueue;
                _usedValueQueue = tmp;
            }

            T currentStdDev = _currentQueue.Dequeue();
            _usedValueQueue.Enqueue(currentStdDev);
        }

        public T GetValue()
        {
            return CurrentValue;
        }
    }
}