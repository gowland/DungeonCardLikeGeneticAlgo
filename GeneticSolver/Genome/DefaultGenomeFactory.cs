namespace GeneticSolver.Genome
{
    public class DefaultGenomeFactory<T> : IGenomeFactory<T>
        where T : new()
    {
        private readonly IGenomeDescription<T> _genomeDescription;

        public DefaultGenomeFactory(IGenomeDescription<T> genomeDescription)
        {
            _genomeDescription = genomeDescription;
        }

        public T GetNewGenome()
        {
            var newGenome = new T();
            foreach (var property in _genomeDescription.Properties)
            {
                property.SetRandom(newGenome);
            }
            return newGenome;
        }
    }
}