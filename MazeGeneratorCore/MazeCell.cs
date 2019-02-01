using System;

namespace MazeGeneratorCore
{
    public class MazeCell : Coords
    {
        public event EventHandler CellChanged;

        public bool WTop { get; set; } = true;
        public bool WLeft { get; set; } = true;
        public bool WRight { get; set; } = true;
        public bool WBot { get; set; } = true;
        public bool Down { get; set; }
        public bool Up { get; set; }
        public string Info { get; set; }
        public int DistanceFromStart { get; set; }

        public bool Visited { get; set; }
        public bool Finished { get; set; }

        public MazeCell(int x, int y, int z) : base(x,y,z)
        {
        }

        public void Update()
        {
            CellChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return (WTop ? "T" : "") + (WBot ? "B" : "") + (WLeft ? "L" : "") + (WRight ? "R" : "");
        }

        internal Coords GetCoords()
        {
            return new Coords(X, Y, Z);
        }
    }

    public class Coords
    {
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public int Z { get; protected set; }

        public Coords(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Adjusts and return self.
        /// </summary>
        /// <param name="direction">A given direction.</param>
        /// <returns>Same object.</returns>
        internal Coords AdjustCoords(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Left:
                    X--;
                    break;
                case MoveDirection.Right:
                    X++;
                    break;
                case MoveDirection.Top:
                    Y--;
                    break;
                case MoveDirection.Bottom:
                    Y++;
                    break;
                case MoveDirection.Up:
                    Z++;
                    break;
                case MoveDirection.Down:
                    Z--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return this;
        }

        public bool ValidateCoordinates(int maxX, int maxY, int maxZ)
        {
            return Y >= 0 && X >= 0 && Z >= 0 && X < maxX && Y < maxY && Z < maxZ;
        }
    }
}