using System;

namespace Game
{
    public class Game
    {
        private readonly Board _board = new Board(3, 3, new MovementResultGenerator());

        public void Set(int x, int y, ICard<CardType> card)
        {
            _board[x, y].Card = card;
        }

        public static Game GetRandomGame()
        {
            var game = new Game();
            return game;
        }
    }
}