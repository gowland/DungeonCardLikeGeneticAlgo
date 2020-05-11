using System.Collections.Generic;
using System.Linq;
using GeneticSolver;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace GeneticAlgo.Coefficients
{
    public class CoefficientsGenomeEvaluator : IGenomeEvaluator<Coefficients, double>
    {
        private readonly IReadOnlyCollection<Point> _pointsToMatch;

        public CoefficientsGenomeEvaluator(IEnumerable<Point> pointsToMatch)
        {
            _pointsToMatch = pointsToMatch.ToArray();
        }

        public IOrderedEnumerable<FitnessResult<Coefficients, double>> GetFitnessResults(IEnumerable<IGenomeInfo<Coefficients>> genomes)
        {
            return SortByDescendingFitness(genomes/*.AsParallel()*/.Select(genome => new FitnessResult<Coefficients, double>(genome, GetError(genome.Genome))));
        }

        public IOrderedEnumerable<Coefficients> GetFitnessResults(IEnumerable<Coefficients> genomes)
        {
            return genomes.OrderBy(GetError);
        }

        private IOrderedEnumerable<FitnessResult<Coefficients, double>> SortByDescendingFitness(IEnumerable<FitnessResult<Coefficients, double>> genomes)
        {
            return genomes.OrderBy(g => g.Fitness);
        }

        private double GetError(Coefficients coefficients)
        {
            return _pointsToMatch
                .Select(point => (point.Y - coefficients.Calc(point.X))/point.Y)
                .Average(error => error * error);
//                return _pointsToMatch
//                    .Average(point => Math.Abs(point.Y - coefficients.Calc(point.X))/point.Y);
        }
    }
}