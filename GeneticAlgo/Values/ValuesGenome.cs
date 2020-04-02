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
    }
}