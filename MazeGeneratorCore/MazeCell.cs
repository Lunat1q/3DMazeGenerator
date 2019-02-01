using System;

namespace MazeGeneratorCore
{
    public class MazeCell
    {
        public event EventHandler CellChanged;

        public bool WTop { get; set; } = true;
        public bool WLeft { get; set; } = true;
        public bool WRight { get; set; } = true;
        public bool WBot { get; set; } = true;
        public bool Floor { get; set; } = true;
        public bool Down { get; set; }
        public bool Up { get; set; }
        public string Info { get; set; }
        public int DistanceFromStart { get; set; }

        public bool Visited { get; set; }
        public bool Finished { get; set; }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public MazeCell(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Update()
        {
            CellChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return (WTop ? "T" : "") + (WBot ? "B" : "") + (WLeft ? "L" : "") + (WRight ? "R" : "");
        }
    }
}