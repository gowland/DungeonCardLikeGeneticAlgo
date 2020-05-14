using System;

namespace Game
{
    public static class GameBuilder
    {
        private static readonly Random Randomizer = new Random(DateTime.Now.Millisecond);
        private static readonly CardType[] LegalRandomStartCardTypes = {CardType.Monster, CardType.Weapon};
        private static readonly CardType[] LegalRandomRewardCardTypes = {CardType.Gold};
        private static readonly MovementResultGenerator MovementResultGenerator = new MovementResultGenerator();
        private const int MinStartCardValue = 1;
        private const int MaxStartCardValue = 8;
        private const int MinRewardCardValue = 1;
        private const int MaxRewardCardValue = 5;

        public static ICard<CardType> GetRandomStartCard()
        {
            return new Card<CardType>(LegalRandomStartCardTypes[Randomizer.Next(0, LegalRandomStartCardTypes.Length)] , Randomizer.Next(MinStartCardValue, MaxStartCardValue));
        }

        public static ICard<CardType> GetRandomRewardCard()
        {
            return new Card<CardType>(LegalRandomRewardCardTypes[Randomizer.Next(0, LegalRandomRewardCardTypes.Length)] , Randomizer.Next(MinRewardCardValue, MaxRewardCardValue));
        }

        private static ICard<CardType> GetDefaultPlayerCard()
        {
            return new Card<CardType>(CardType.Player, 10);
        }

        public static Board GetRandomStartBoard()
        {
            var board = new Board(3, 3, MovementResultGenerator);

            board.ResetBoard(GetRandomStartCard, GetDefaultPlayerCard());

            return board;
        }

        public static void RandomizeBoardToStart(Board board)
        {
            board.ResetBoard(GetRandomStartCard, GetDefaultPlayerCard());
        }
    }
}