namespace GeneticSolver
{
    public class DefaultGenomeFactory<T> : IGenomeFactory<T>
        where T : new()
    {
        public T GetNewGenome()
        {
            return new T();
        }
    }
}