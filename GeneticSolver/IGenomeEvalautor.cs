namespace GeneticSolver
{
    public interface IGenomeEvalautor<T>
    {
        int GetFitness(IGenome<T> genome);
    }
}