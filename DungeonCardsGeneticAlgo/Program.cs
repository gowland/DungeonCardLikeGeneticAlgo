﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GeneticSolver;
using System.Text;
using System.Threading.Tasks;
using DungeonCardsGeneticAlgo.Support;
using DungeonCardsGeneticAlgo.Support.WithLogic;
using Game;
using GeneticSolver.BreedingStrategies;
using GeneticSolver.Expressions;
using GeneticSolver.Expressions.Implementations;
using GeneticSolver.Mutators;
using GeneticSolver.Random;
using GeneticSolver.ReproductionStrategies;
using GeneticSolver.RequiredInterfaces;

namespace DungeonCardsGeneticAlgo
{
    class Program
    {
        static void Main(string[] args)
        {
            var maxEliteSize = 1000;
            var cache = new FitnessCache<GameAgentLogicGenome, double>(2000*maxEliteSize); // TODO: need to clear on repeated runs
            var evaluator = new GameAgentEvaluator<GameAgentLogicGenome>(cache, genome => new GameAgentWithLogic(genome));

            var solverParameters = new SolverParameters(
                maxEliteSize,
                2*maxEliteSize,
                0.3);

            var expressionGenerator = new ExpressionGenerator<GameState>(
                new BellWeightedRandom(0.5),
                new IExpression<GameState>[]
                {
                    new BoundValueExpression<GameState>(game => game.HeroGold, nameof(GameState.HeroGold)),
                    new BoundValueExpression<GameState>(game => game.HeroHealth, nameof(GameState.HeroHealth)),
                    new BoundValueExpression<GameState>(game => game.HeroWeapon, nameof(GameState.HeroWeapon)),
                    new BoundValueExpression<GameState>(game => game.CardGold, nameof(GameState.CardGold)),
                    new BoundValueExpression<GameState>(game => game.MonsterHealth, nameof(GameState.MonsterHealth)),
                    new BoundValueExpression<GameState>(game => game.CardWeapon, nameof(GameState.CardWeapon)),
                },
                new []
                {
                    new Operation((a,b) => a + b, "+"),
                    new Operation((a,b) => a - b, "-"),
                    new Operation((a,b) => a * b, "*"),
                });

            // TestExpressionGenerator(expressionGenerator);

//            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
//                tasks.Add(Task.Run(() => LaunchEvolutionRun(genomeDescriptions, solverParameters, defaultGenomeFactory, evaluator)));
                LaunchEvolutionRun(solverParameters, evaluator, expressionGenerator);
            }

//            Task.WaitAll(tasks.ToArray());

//            TestBellCurve();
            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static void TestExpressionGenerator(ExpressionGenerator<GameState> expressionGenerator)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(expressionGenerator.GetRandomExpression());
            }
        }

        private static void LaunchEvolutionRun(SolverParameters solverParameters, GameAgentEvaluator<GameAgentLogicGenome> evaluator, ExpressionGenerator<GameState> expressionGenerator)
        {
            var bellWeightedRandom = new CyclableBellWeightedRandom(10.0, 3.0, 1.0, 0.5, 0.1);
            var genomeDescriptions = new GameAgentLogicGenomeDescription(bellWeightedRandom, expressionGenerator);
            var defaultGenomeFactory = new GeneticSolver.Genome.DefaultGenomeFactory<GameAgentLogicGenome>(genomeDescriptions);
            var mutationProbabilities = new Cyclable<double>(new[]{0.3, 0.9, 1.0});
            var mutator = new GenomeMutator<GameAgentLogicGenome>(genomeDescriptions, mutationProbabilities, new UnWeightedRandom());
            var logger = new GameAgentWithLogicSolverLogger();
            var solver = new Solver<GameAgentLogicGenome, double>(
                defaultGenomeFactory,
                evaluator,
                logger,
                solverParameters,
                new IEarlyStoppingCondition<GameAgentLogicGenome, double>[]
                {
                    // new GeneticSolver.EarlyStoppingConditions.FitnessThresholdReachedEarlyStopCondition<GameAgentMultipliers, double>(fitness => fitness < 1e-6),
                    new GeneticSolver.EarlyStoppingConditions.ProgressStalledEarlyStoppingCondition<GameAgentLogicGenome, double>(10, 0.5, 0.8),
                    new GeneticSolver.EarlyStoppingConditions.FitnessNotImprovingEarlyStoppingCondition<GameAgentLogicGenome>(1, 100),
                },
                new IGenomeReproductionStrategy<GameAgentLogicGenome>[]
                {
                    new CyclableReproductionStrategy<GameAgentLogicGenome>(new IGenomeReproductionStrategy<GameAgentLogicGenome>[]
                    {
                        new SexualGenomeReproductionStrategy<GameAgentLogicGenome, double>(mutator, new StratifiedBreedingStrategy(),
                            defaultGenomeFactory, genomeDescriptions, evaluator, 40, 2),
                        new SexualGenomeReproductionStrategy<GameAgentLogicGenome, double>(mutator, new GeneticSolver.PairingStrategies.RandomBreedingStrategy(),
                            defaultGenomeFactory, genomeDescriptions, evaluator, 40, 2),
                        new AsexualGenomeReproductionStrategy<GameAgentLogicGenome>(new GenomeMutator<GameAgentLogicGenome>(genomeDescriptions, 1.0, new UnWeightedRandom())),
                    }),
                    new AsexualGenomeReproductionStrategy<GameAgentLogicGenome>(new GenomeMutator<GameAgentLogicGenome>(genomeDescriptions, 0.4, new UnWeightedRandom())),
                });
            solver.NewGeneration += (s, e) => bellWeightedRandom.CycleStdDev();
            solver.NewGeneration += (s, e) => mutationProbabilities.Cycle();

            logger.Start();
            var best = solver.Evolve(1000);
            logger.LogGeneration(best);
            logger.End();
        }
    }

    public class CyclableReproductionStrategy<T> : Cyclable<IGenomeReproductionStrategy<T>>, IGenomeReproductionStrategy<T>
    {
        public CyclableReproductionStrategy(IEnumerable<IGenomeReproductionStrategy<T>> values) : base(values)
        {
        }

        public IEnumerable<T> ProduceOffspring(IEnumerable<T> parents)
        {
            return CurrentValue.ProduceOffspring(parents);
        }
    }
}
