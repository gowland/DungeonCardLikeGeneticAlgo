namespace GeneticSolver
{
    public interface IGenome<out T>
    {
        T Value { get; }
    }
}