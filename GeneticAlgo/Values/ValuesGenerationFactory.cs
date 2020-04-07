using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class ValuesGenerationFactory : IGenerationFactory<Values>
    {
        public Values GetNewGenome()
        {
            return new Values();
        }
    }
}