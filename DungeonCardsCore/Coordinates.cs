using System;
using System.Collections.Generic;

namespace Game
{
    public class Coordinates : IEquatable<Coordinates>
    {
        private static readonly IDictionary<Direction, CoordinateAdjustments> DirectionToXyAdjustment = new Dictionary<Direction, CoordinateAdjustments>
        {
            [Direction.Up] = new CoordinateAdjustments(0, -1),
            [Direction.Down] = new CoordinateAdjustments(0, 1),
            [Direction.Left] = new CoordinateAdjustments(-1, 0),
            [Direction.Right] = new CoordinateAdjustments(1, 0),
        };

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X { get; }
        public int Y { get; }

        public Coordinates Get(Direction direction)
        {
            CoordinateAdjustments adjustment = DirectionToXyAdjustment[direction];
            return new Coordinates(X + adjustment.X, Y + adjustment.Y);
        }

        public bool Equals(Coordinates other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Coordinates) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}