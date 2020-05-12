using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Game
{
    public class Grid<T>
    {
        private readonly T[] _items;

        public Grid(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                throw  new ArgumentException($"Size must be positive, non-zero, but was {width},{height}");
            }
            _items = new T[width * height];
            Width = width;
            Height = height;
        }

        public Grid(int width, int height, IEnumerable<T> items)
            : this(width, height)
        {
            if (width < 1 || height < 1)
            {
                throw  new ArgumentException($"Size must be positive, non-zero, but was {width},{height}");
            }

            Width = width;
            Height = height;

            _items = items.ToArray();

            if (_items.Length != width * height)
            {
                throw  new ArgumentException($"Expected size of array to be {width*height} but was {_items.Length}");
            }
        }

        public T this[int x, int y]
        {
            get => _items[CoordinatesToIndex(x, y)];
            set => _items[CoordinatesToIndex(x, y)] = value;
        }

        public T this[Coordinates coordinates]
        {
            get => this[coordinates.X, coordinates.Y];
            set => this[coordinates.X, coordinates.Y] = value;
        }

        public int Width { get; }

        public int Height { get; }

        private int CoordinatesToIndex(int x, int y)
        {
            if (x >= Width || y >= Height || x < 0 || y < 0)
            {
                throw new ArgumentException($"Coordinates must fall within 0,0 -> {Width-1},{Height-1}, but were {x},{y}");
            }

            return y * Width + x;
        }

        public bool CoordinatesInBounds(Coordinates position)
        {
            return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
        }

        public IEnumerable<T> GetAllCardsNextTo(Coordinates position, Direction direction)
        {
            bool inBounds = false;
            do
            {
                var nextCoordinate = position.Get(direction);
                inBounds = CoordinatesInBounds(nextCoordinate);
                if (inBounds) yield return this[nextCoordinate];
                position = nextCoordinate;
            } while (inBounds);
        }
    }
}