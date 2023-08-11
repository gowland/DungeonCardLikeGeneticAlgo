using GeneticSolver.Interfaces;

namespace GeneticSolver
{
    public class FitnessResult<T, TScore>
    {
        public FitnessResult(IGenomeInfo<T> genomeInfo, TScore fitness)
        {
            GenomeInfo = genomeInfo;
            Fitness = fitness;
        }

        public IGenomeInfo<T> GenomeInfo { get; }
        public TScore Fitness { get; }
    }
}