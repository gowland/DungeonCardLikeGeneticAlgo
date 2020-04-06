using System.Collections.Generic;
using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class ValuesGenome : IGenome<Values>
    {
        public ValuesGenome(Values values)
        {
            Value = values;
        }

        public Values Value { get; }

        public IEnumerable<IGenomeProperty> Properties => new[]
        {
            new IntegerGenomeProperty(() => Value.A, val => Value.A = val, 0, 1000, -30, 30),
            new IntegerGenomeProperty(() => Value.B, val => Value.B = val, 0, 1000, -30, 30),
            new IntegerGenomeProperty(() => Value.C, val => Value.C = val, 0, 1000, -30, 30),
            new IntegerGenomeProperty(() => Value.D, val => Value.D = val, 0, 1000, -30, 30),
            new IntegerGenomeProperty(() => Value.E, val => Value.E = val, 0, 1000, -30, 30),
        };
    }
}