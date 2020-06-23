using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Player;
using GeneticSolver;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace DungeonCardsGeneticAlgo.Support
{
    public class GameAgentEvaluator<T> : IGenomeEvaluator<T, double>
    {
        private readonly FitnessCache<T, double> _fitnessCache;
        private readonly Func<T, IGameAgent> _gameAgentFunc;
        private readonly Board _board = GameBuilder.GetRandomStartBoard();

        public GameAgentEvaluator(FitnessCache<T, double> fitnessCache, Func<T, IGameAgent> gameAgentFunc)
        {
            _fitnessCache = fitnessCache;
            _gameAgentFunc = gameAgentFunc;
        }

        public IOrderedEnumerable<FitnessResult<T, double>> GetFitnessResults(IEnumerable<IGenomeInfo<T>> genomes)
        {
            return genomes.Select(genome => new FitnessResult<T, double>(genome, GetFitness(genome.Genome)))
                .OrderByDescending(r => r.Fitness);
        }

        public IOrderedEnumerable<T> GetFitnessResults(IEnumerable<T> genomes)
        {
            return genomes.OrderByDescending(GetFitness);
        }

        private double GetFitness(T genome)
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

        private int DoOneRun(T multipliers)
        {
            GameBuilder.RandomizeBoardToStart(_board);
            var gameAgent = _gameAgentFunc(multipliers);
            var gameRunner = new GameRunner(gameAgent, _ => {});
            return gameRunner.RunGame(_board);
        }
    }
}