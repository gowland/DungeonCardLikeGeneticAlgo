namespace GeneticSolver
{
    public class FitnessResult<T>
    {
        public FitnessResult(IGenome<T> genome, int fitness)
        {
            Genome = genome;
            Fitness = fitness;
        }

        public IGenome<T> Genome { get; }
        public int Fitness { get; }
    }
}