using System;
using System.Linq;

namespace Game.Player
{
    public class MaximalMoveSelector : IMoveSelector
    {
        public DirectionResult GetDirectionFromAlgo(Board board, Func<Board,ISlot<ICard<CardType>>,double> getScore)
        {
            var moves = board.GetCurrentLegalMoves();

            var scoredMoves = moves.Select(pair => new { Direction = pair.Key, Score = getScore(board, pair.Value) });

            return new DirectionResult(scoredMoves.OrderByDescending(move => move.Score).First().Direction);
        }
    }

    public interface IMoveSelector
    {
        DirectionResult GetDirectionFromAlgo(Board board, Func<Board,ISlot<ICard<CardType>>,double> getScore);
    }

    public class ProbabilityBasedMoveSelector : IMoveSelector
    {
        private readonly Random _random = new Random();

        public DirectionResult GetDirectionFromAlgo(Board board, Func<Board,ISlot<ICard<CardType>>,double> getScore)
        {
            var moves = board.GetCurrentLegalMoves();

            var scoredMoves = moves.Select(pair => new { Direction = pair.Key, Score = getScore(board, pair.Value) }).ToArray();

            var scoresTotal = scoredMoves.Sum(r => r.Score);

            var scoredMovesPct = scoredMoves.Select(m => new {Direction = m.Direction, Score = m.Score / scoresTotal}).ToArray();

            var rand = _random.NextDouble();
            double totalValuePassed = 0.0;

            foreach (var move in scoredMovesPct)
            {
                totalValuePassed += move.Score;
                if (rand < totalValuePassed)
                {
                    return new DirectionResult(move.Direction);
                }
            }

            return new DirectionResult(scoredMovesPct.Last().Direction);
        }
    }
}