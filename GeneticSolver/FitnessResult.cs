namespace GeneticSolver
{
    public class FitnessResult<T, TScore>
    {
        public FitnessResult(T genome, TScore fitness)
        {
            Genome = genome;
            Fitness = fitness;
        }

        public T Genome { get; }
        public TScore Fitness { get; }
    }
}