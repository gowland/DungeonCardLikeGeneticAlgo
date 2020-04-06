using System.Collections.Generic;
using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class ValuesGenome : IGenome<Values>
    {
        public IEnumerable<IGenomeProperty<Values>> Properties => new[]
        {
            new IntegerGenomeProperty<Values>(g => g.A, (g, val) => g.A = val, 0, 1000, -30, 30),
            new IntegerGenomeProperty<Values>(g => g.B, (g, val) => g.B = val, 0, 1000, -30, 30),
            new IntegerGenomeProperty<Values>(g => g.C, (g, val) => g.C = val, 0, 1000, -30, 30),
            new IntegerGenomeProperty<Values>(g => g.D, (g, val) => g.D = val, 0, 1000, -30, 30),
            new IntegerGenomeProperty<Values>(g => g.E, (g, val) => g.E = val, 0, 1000, -30, 30),
        };
    }
}