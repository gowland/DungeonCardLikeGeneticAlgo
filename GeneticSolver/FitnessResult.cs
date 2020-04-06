namespace GeneticSolver
{
    public class FitnessResult<T>
    {
        public FitnessResult(T genome, int fitness)
        {
            Genome = genome;
            Fitness = fitness;
        }

        public T Genome { get; }
        public int Fitness { get; }
    }
}