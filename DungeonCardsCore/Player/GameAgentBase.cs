using System.Linq;

namespace Game.Player
{
    public abstract class GameAgentBase : IGameAgent
    {
        public DirectionResult GetDirectionFromAlgo(Board board)
        {
            var moves = board.GetCurrentLegalMoves();

            var scoredMoves = moves.Select(pair => new { Direction = pair.Key, Score = GetScore(board, pair.Value) });

            return new DirectionResult(scoredMoves.OrderByDescending(move => move.Score).First().Direction);
        }

        protected abstract double GetScore(Board board, ISlot<ICard<CardType>> slot);
    }
}