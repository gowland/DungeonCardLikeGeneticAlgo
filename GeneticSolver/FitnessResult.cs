namespace GeneticSolver
{
    public class FitnessResult<T, TScore>
    {
        public FitnessResult(IGenomeInfo<T> genome, TScore fitness)
        {
            Genome = genome;
            Fitness = fitness;
        }

        public IGenomeInfo<T> Genome { get; }
        public TScore Fitness { get; }
    }
}