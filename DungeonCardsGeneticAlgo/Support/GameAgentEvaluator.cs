using System.Collections.Generic;
using System.Linq;
using Game;
using GeneticSolver;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace DungeonCardsGeneticAlgo.Support
{
    public class GameAgentEvaluator : IGenomeEvaluator<GameAgentMultipliers, double>
    {
        private readonly FitnessCache<GameAgentMultipliers, double> _fitnessCache;
        private readonly Board _board = GameBuilder.GetRandomStartBoard();

        public GameAgentEvaluator(FitnessCache<GameAgentMultipliers, double> fitnessCache)
        {
            _fitnessCache = fitnessCache;
        }

        public IOrderedEnumerable<FitnessResult<GameAgentMultipliers, double>> GetFitnessResults(IEnumerable<IGenomeInfo<GameAgentMultipliers>> genomes)
        {
            return genomes.Select(genome => new FitnessResult<GameAgentMultipliers, double>(genome, GetFitness(genome.Genome)))
                .OrderByDescending(r => r.Fitness);
        }

        public IOrderedEnumerable<GameAgentMultipliers> GetFitnessResults(IEnumerable<GameAgentMultipliers> genomes)
        {
            return genomes.OrderByDescending(GetFitness);
        }

        private double GetFitness(GameAgentMultipliers genome)
        {
            if (_fitnessCache.TryGetFitness(genome, out double cachedFitness))
            {
                return cachedFitness;
            }

            var fitnesses = Enumerable.Range(1, 100)
                .Select(_ => (double)DoOneRun(genome))
                .ToArray();

            var fitness = fitnesses.Average() + fitnesses.StdDev();

            _fitnessCache.Cache(genome, fitness);

            return fitness;
        }

        private int DoOneRun(GameAgentMultipliers multipliers)
        {
            GameBuilder.RandomizeBoardToStart(_board);
            var gameAgent = new GameAgent(multipliers);
            var gameRunner = new GameRunner(gameAgent.GetDirectionFromAlgo, _ => {});
            return gameRunner.RunGame(_board);
        }
    }
}