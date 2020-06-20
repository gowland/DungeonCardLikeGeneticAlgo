using System.Collections.Generic;
using GeneticSolver;
using GeneticSolver.GenomeProperty;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace GeneticAlgo.Coefficients
{
    public class CoefficientsGenomeDescriptions : IGenomeDescription<Coefficients>
    {
        private readonly IRandom _random;
        private readonly double _minChange = -30;
        private readonly double _maxChange = 30;

        public CoefficientsGenomeDescriptions(IRandom random)
        {
            _random = random;
        }

        public IEnumerable<IGenomeProperty<Coefficients>> Properties => new[]
        {
            new DoubleGenomeProperty<Coefficients>(g => g.FifthLevel, (g, val) => g.FifthLevel = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<Coefficients>(g => g.FourthLevel, (g, val) => g.FourthLevel = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<Coefficients>(g => g.ThirdLevel, (g, val) => g.ThirdLevel = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<Coefficients>(g => g.SecondLevel, (g, val) => g.SecondLevel = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<Coefficients>(g => g.FirstLevel, (g, val) => g.FirstLevel = val, -100, 100, _minChange, _maxChange, _random),
        };
    }
}