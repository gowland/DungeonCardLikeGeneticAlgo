using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public enum SquareDesc
    {
        Center = 0,
        Edge,
        Corner,
    }
    public class Board
    {
        private readonly Grid<Slot<ICard<CardType>>> _grid;
        private readonly IMovementResultGenerator<ICard<CardType>> _movementResultGenerator;
        private readonly Hero _hero = new Hero();
        private Coordinates _playerCoordinates = new Coordinates(1, 1);
        private readonly IDictionary<Coordinates, IDictionary<Direction, Slot<ICard<CardType>>>> _legalMovesCache;

        public Board(int width, int height, IMovementResultGenerator<ICard<CardType>> movementResultGenerator)
        {
            _movementResultGenerator = movementResultGenerator;
            _grid = new Grid<Slot<ICard<CardType>>>(width, height, Enumerable.Range(0, width * height).Select(_ => new Slot<ICard<CardType>>()));
            _legalMovesCache = GetLegalMovesCache();
        }

        public ISlot<ICard<CardType>> this[int x, int y] => _grid[x, y];

        public ISlot<ICard<CardType>> this[Coordinates coordinate] => _grid[coordinate.X, coordinate.Y];

        public int Gold => _hero.Gold;

        public int Weapon => _hero.Weapon;

        public int HeroHealth => this[_playerCoordinates].Card.Value;

        public void ResetBoard(Func<ICard<CardType>> getCardFunc, ICard<CardType> playerCard)
        {
            _playerCoordinates = new Coordinates(1, 1);
            _hero.Reset();

            foreach (Coordinates coordinates in _grid.GetAllPositions())
            {
                _grid[coordinates].Card = coordinates.Equals(_playerCoordinates)
                    ? playerCard
                    : getCardFunc.Invoke();
            }

        }

        public IDictionary<Direction, Slot<ICard<CardType>>> GetCurrentLegalMoves()
        {
            return _legalMovesCache[_playerCoordinates];
        }

        private IDictionary<Coordinates, IDictionary<Direction, Slot<ICard<CardType>>>> GetLegalMovesCache()
        {
            return _grid.GetAllPositions()
                .ToDictionary(coords => coords, GetLegalMovesForPosition);
        }

        private IDictionary<Direction, Slot<ICard<CardType>>> GetLegalMovesForPosition(Coordinates coordinates)
        {
            var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>();
            var directionSlots = directions
                .Select(dir => new {Direction = dir, Slot = GetSlot(coordinates.Get(dir))})
                .Where(dir => dir.Slot != null)
                .ToDictionary(dir => dir.Direction, dir => dir.Slot);
            return directionSlots;
        }

        public void TakeAction(Direction direction)
        {
            Coordinates nextCoordinates = _playerCoordinates.Get(direction);
            var newSlot = GetSlot(nextCoordinates);
            var originalSlot = GetSlot(_playerCoordinates);

            if (newSlot == null)
            {
                Console.WriteLine("Where you going bud?");
                return;
            }

            if (originalSlot == null)
            {
                throw new Exception("Whaaaaa?");
            }

            var heroCard = originalSlot.Card;
            var nextCard = newSlot.Card;
            var movementResult = _movementResultGenerator.GetResult(nextCard);

            if (movementResult == MovementResult.Collect)
            {
                Collect(_hero, nextCard, newSlot, heroCard, originalSlot);

                FillGap(_playerCoordinates, direction, originalSlot);

                _playerCoordinates = nextCoordinates;
            }
            else if (movementResult == MovementResult.Attack)
            {
                Attack(_hero, nextCard, heroCard, newSlot);
            }
            else
            {
                throw new Exception($"Movement result not handled: {movementResult}");
            }
        }

        private static void Collect(Hero hero, ICard<CardType> nextCard, Slot<ICard<CardType>> newSlot, ICard<CardType> heroCard, Slot<ICard<CardType>> originalSlot)
        {
            switch (nextCard.Type)
            {
                case CardType.Gold:
                    hero.AddGold(nextCard.Value);
                    break;
                case CardType.Weapon:
                    hero.PickupWeapon(nextCard.Value);
                    break;
            }

            newSlot.Card = heroCard;
            originalSlot.Card = null;
        }

        private static void Attack(Hero hero, ICard<CardType> nextCard, ICard<CardType> heroCard, Slot<ICard<CardType>> newSlot)
        {
            if (hero.Weapon > 0)
            {
                int heroWeapon = hero.Weapon;
                int monsterHealth = nextCard.Value;
                int minValue = Math.Min(heroWeapon, monsterHealth);

                hero.UseWeapon(minValue);
                nextCard.Value -= minValue;
            }
            else
            {
                int heroHealth = heroCard.Value;
                int monsterHealth = nextCard.Value;
                int minValue = Math.Min(heroHealth, monsterHealth);

                heroCard.Value -= minValue;
                nextCard.Value -= minValue;
            }

            if (nextCard.Value <= 0)
            {
                newSlot.Card = GameBuilder.GetRandomRewardCard();
            }
        }

        private void FillGap(Coordinates playerCoordinates, Direction direction, Slot<ICard<CardType>> originalSlot)
        {
            Direction oppositeDirection = direction.GetOpposite();
            Direction perpendicularDirection = direction.GetPerpendicular();

            if (_grid.CoordinatesInBounds(playerCoordinates.Get(oppositeDirection)))
            {
                Coordinates nextCardCoordinates = playerCoordinates.Get(oppositeDirection);
                var nextCardSlot = GetSlot(nextCardCoordinates);
                originalSlot.Card = nextCardSlot.Card;
                nextCardSlot.Card = GameBuilder.GetRandomStartCard();
            }
            else
            {
                Slot<ICard<CardType>> currentSlot = originalSlot;
                var nextSlots = _grid.GetAllCardsNextTo(playerCoordinates, perpendicularDirection);

                foreach (var nextSlot in nextSlots)
                {
                    currentSlot.Card = nextSlot.Card;
                    currentSlot = nextSlot;
                }

                currentSlot.Card = GameBuilder.GetRandomStartCard();
            }
        }

        public SquareDesc Desc()
        {
            if (_playerCoordinates.X == 0 || _playerCoordinates.X == 2)
            {
                if (_playerCoordinates.Y == 0 || _playerCoordinates.Y == 2)
                {
                    return SquareDesc.Corner;
                }

                return SquareDesc.Edge;
            }

            return SquareDesc.Center;
        }

        public Slot<ICard<CardType>> GetSlot(Coordinates coordinates)
        {
            try
            {
                return _grid[coordinates];

            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }

            return null;
        }
    }
}