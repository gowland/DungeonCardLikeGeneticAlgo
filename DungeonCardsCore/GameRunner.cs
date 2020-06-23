using System;
using Game.Player;

namespace Game
{
    public class GameRunner
    {
        private Func<Board, DirectionResult> _getDirectionFunc;
        private Action<Board> _dumpBoardAction;
        public event EventHandler StateChanged;
        public event EventHandler<Direction> DirectionChosen;

        public GameRunner(IGameAgent gameAgent, Action<Board> dumpBoardAction)
        {
            _dumpBoardAction = dumpBoardAction;
            _getDirectionFunc = gameAgent.GetDirectionFromAlgo;
        }

        public int RunGame(Board board)
        {
            do
            {
                _dumpBoardAction(board);

                // Checking hero's health
                if (board.HeroHealth <= 0)
                {
                    // Console.WriteLine("Game Over! You suck :-)");
                    return board.Gold;
                }

                var directionResult = _getDirectionFunc(board);
                if (!directionResult.Success) return board.Gold;

                if (directionResult.Direction.HasValue)
                {
                    OnDirectionChosen(directionResult.Direction.Value);
                    board.TakeAction(directionResult.Direction.Value);
                    OnStateChanged();
                }
                else
                {
                    Console.WriteLine("Unknown command");
                }
            } while (true);
        }

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDirectionChosen(Direction e)
        {
            DirectionChosen?.Invoke(this, e);
        }
    }
}