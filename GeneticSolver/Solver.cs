using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Genome;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public class Solver<T, TScore>
        where T : ICloneable
        where TScore : IComparable<TScore>
    {
        private readonly IGenomeFactory<T> _genomeFactory;
        private readonly IGenomeEvaluator<T, TScore> _evaluator;
        private readonly ISolverLogger<T, TScore> _logger;
        private readonly ISolverParameters _solverParameters;
        private readonly IEnumerable<IEarlyStoppingCondition<T, TScore>> _earlyStoppingConditions;
        private readonly IEnumerable<IGenomeReproductionStrategy<T>> _genomeReproductionStrategies;

        public event EventHandler<GenerationResult<T, TScore>> NewGeneration;

        public Solver(IGenomeFactory<T> genomeFactory,
            IGenomeEvaluator<T, TScore> evaluator,
            ISolverLogger<T, TScore> logger, ISolverParameters solverParameters,
            IEnumerable<IEarlyStoppingCondition<T, TScore>> earlyStoppingConditions,
            IEnumerable<IGenomeReproductionStrategy<T>> genomeReproductionStrategies)
        {
            _genomeFactory = genomeFactory;
            _evaluator = evaluator;
            _logger = logger;
            _solverParameters = solverParameters;
            _earlyStoppingConditions = earlyStoppingConditions;
            _genomeReproductionStrategies = genomeReproductionStrategies;
        }

        public IGenerationResult<T, TScore> Evolve(int iterations, IEnumerable<T> originalGeneration = null)
        {
            GenerationResult<T, TScore> generationResult = null;

            var originalGenomes = originalGeneration ?? _genomeFactory.GetNewGenomes(_solverParameters.InitialGenerationSize);
            var scoredGeneration = new ScoredGeneration<T, TScore>(originalGenomes.Select(g => new GenomeInfo<T>(g, 0)), _evaluator);


            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                _logger.LogStartGeneration(generationNum);

                IEnumerable<IGenomeInfo<T>> elite = scoredGeneration.OrderedGenomes.Take(_solverParameters.MaxEliteSize);


                var num = generationNum;
                var children = _genomeReproductionStrategies
                    .SelectMany(reproductionStrategy =>
                        reproductionStrategy.ProduceOffspring(elite.Select(g => g.Genome)))
                    .Select(g => new GenomeInfo<T>(g, num));

                var nextGenerationGenomes = elite.Concat(children).ToArray();

                scoredGeneration = new ScoredGeneration<T, TScore>(nextGenerationGenomes, _evaluator);

                generationResult = new GenerationResult<T, TScore>(generationNum, scoredGeneration);

                OnNewGeneration(generationResult);
                _logger.LogGenerationInfo(generationResult);

                if (IsEarlyStopConditionHit(generationResult))
                {
                    return generationResult;
                }
            }

            return generationResult;
        }

        private bool IsEarlyStopConditionHit(GenerationResult<T, TScore> generationResult)
        {
            return _earlyStoppingConditions.Any(condition =>
                condition.Match(generationResult));
        }

        protected virtual void OnNewGeneration(GenerationResult<T, TScore> e)
        {
            NewGeneration?.Invoke(this, e);
        }
    }
}