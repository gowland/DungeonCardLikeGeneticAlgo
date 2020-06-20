using DungeonCardsGeneticAlgo.Support.WithLogic;
using GeneticSolver.Expressions;
using GeneticSolver.Expressions.Implementations;
using GeneticSolver.Random;

namespace DungeonCardsGeneticAlgo.Support
{
    public static class ExpressionGeneratorFactory
    {
        public static ExpressionGenerator<GameState> CreateExpressionGenerator()
        {
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
                new[]
                {
                    new Operation((a, b) => a + b, "+"),
                    new Operation((a, b) => a - b, "-"),
                    new Operation((a, b) => a * b, "*"),
                });
            return expressionGenerator;
        }
    }
}