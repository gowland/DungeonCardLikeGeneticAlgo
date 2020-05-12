using System.ComponentModel;

namespace Game
{
    internal static class DirectionExtensions
    {
        public static Direction GetOpposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Right:
                    return Direction.Left;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public static Direction GetPerpendicular(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Left;
                case Direction.Down:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Down;
                case Direction.Left:
                    return Direction.Up;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}