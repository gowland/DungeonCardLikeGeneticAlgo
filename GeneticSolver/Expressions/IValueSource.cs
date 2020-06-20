namespace GeneticSolver.Expressions
{
    public interface IValueSource<out T>
    {
        T GetValue();
    }
}