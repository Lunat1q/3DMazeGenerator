using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MazeGeneratorCore
{
    public sealed class Maze
    {
        private int SizeX { get; }
        private int SizeY { get; }
        private int SizeZ { get; }
        private readonly MazeGenerationType _genType;

        public MazeCell[,,] CompleteMaze { get; set; } // TODO: Rewrite to linked cell to allow any configuration of the grid.
        public int NextCellDelay { get; set; } = 0;

        public EventHandler MazeGenerated;

        public Maze(int xSize, int ySize, int zSize, MazeGenerationType genType = 0)
        {
            SizeX = xSize;
            SizeY = ySize;
            SizeZ = zSize;
            _genType = genType;
            CompleteMaze = new MazeCell[xSize, ySize, zSize];
            for (var x = 0; x < xSize; x++)
            {
                for (var y = 0; y < ySize; y++)
                {
                    for (var z = 0; z < zSize; z++)
                    {
                        CompleteMaze[x, y, z] = new MazeCell(x, y, z);
                    }
                }
            }
        }

        public async void GenerateMaze(MazeCell startCell = null, Random randomGenerator = null)
        {
            var rnd = randomGenerator ?? new Random();
            //Random Generation Start Point attempt if first cell is not set;
            var firstCell = startCell ?? CompleteMaze[rnd.Next(SizeX), rnd.Next(SizeY), rnd.Next(SizeZ)];

            firstCell.Info = "Start";
            firstCell.Visited = true;

            var movementList = new List<MazeCell> {firstCell};

            //TEST - making UP/Down ways much less to appear (attempt at least) :P
            var floorChanceInit = SizeX * SizeY * SizeZ / 50 > 0 ? SizeX * SizeY * SizeZ / 50 : 1;
            var floorChance = floorChanceInit;
            var floorSwitch = 1;
            do
            {
                var index = GetNextCellIndexToMoveFrom(movementList, rnd);
                var currentCell = movementList[index];
                var dir = GetNextMove(rnd, floorChance, currentCell, out var nextMazeCell);

                if (nextMazeCell == null)
                {
                    TryToUnstuck(movementList, currentCell, index);
                }
                else
                {
                    RemoveCellsWithNoPossibleMoves(movementList);

                    if (dir == MoveDirection.Up || dir == MoveDirection.Down)
                    {
                        floorSwitch++;
                        floorChance *= floorSwitch;
                    }
                    else
                    {
                        if (floorChance > 100)
                        {
                            floorChance = (int)Math.Sqrt(floorChance);
                        }
                        else if (floorChance > floorChanceInit * 2)
                        {
                            floorChance = floorChance - 1 > 0 ? floorChance - 1 : 3;
                        }
                    }

                    AdjustWallsAndLadders(dir, currentCell, nextMazeCell);
                    ProceedCell(nextMazeCell, currentCell.DistanceFromStart + 1, movementList);
                }
                currentCell.Update();

                if (NextCellDelay > 0)
                {
                    await Task.Delay(NextCellDelay);
                }
            } while (movementList.Count > 0);

            MazeGenerated?.Invoke(this, EventArgs.Empty);
        }

        private static void ProceedCell(MazeCell nextMazeCell, int distance, ICollection<MazeCell> movementList)
        {
            nextMazeCell.Visited = true;
            nextMazeCell.DistanceFromStart = distance;
            nextMazeCell.Update();
            movementList.Add(nextMazeCell);
        }

        private MoveDirection GetNextMove(Random rnd, int floorChance, MazeCell currentCell, out MazeCell nextMazeCell)
        {
            var possibleMoves = Enumerable.Range(0, 5).OrderBy(x => rnd.Next()).Take(5);

            MoveDirection dir = 0;
            nextMazeCell = null;
            foreach (var item in possibleMoves)
            {
                dir = GetNextMoveDirection(item, rnd, floorChance);
                if (dir == MoveDirection.None) continue;

                var nextCoords = currentCell.GetCoords().AdjustCoords(dir);

                if (nextCoords.ValidateCoordinates(SizeX, SizeY, SizeZ))
                {
                    if (TryGetNextCellToMove(nextCoords, currentCell, out nextMazeCell)) break;
                }
            }

            return dir;
        }

        private bool TryGetNextCellToMove(Coords nextCoords, MazeCell currentCell, out MazeCell mazeCell)
        {
            mazeCell = CompleteMaze[nextCoords.X, nextCoords.Y, nextCoords.Z];
            if (!mazeCell.Visited) // prevent spawn of 2 ladders on top of each other
            {
                if (nextCoords.Z - currentCell.Z > 0 && !mazeCell.Down ||
                    nextCoords.Z - currentCell.Z < 0 && !currentCell.Down ||
                    nextCoords.Z - currentCell.Z == 0)
                {
                    if (nextCoords.Z - currentCell.Z > 0 && nextCoords.Z > 1)
                    {
                        if (!CompleteMaze[nextCoords.X, nextCoords.Y, nextCoords.Z - 2].Down) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            mazeCell = null;
            return false;
        }

        private void RemoveCellsWithNoPossibleMoves(List<MazeCell> movementList)
        {
            for (var m = movementList.Count - 1;
                m > 0;
                m--) // Test Algorithm, should increase passage length for 1-4 creation variants
            {
                var cellToCheck = movementList[m];
                if (!CheckAroundAvail(cellToCheck))
                {
                    RemoveCellFromListAndMarkAsDone(cellToCheck, movementList);
                }
            }
        }

        private int GetNextCellIndexToMoveFrom(IReadOnlyCollection<MazeCell> movementList, Random rnd)
        {
            int index;
            switch (_genType)
            {
                case MazeGenerationType.Latest:
                    //Latest
                    index = movementList.Count - 1;
                    break;
                case MazeGenerationType.Random:
                    //Random!
                    index = rnd.Next(movementList.Count);
                    break;
                case MazeGenerationType.HalfHalf:
                    index = rnd.Next(2) == 1 ? rnd.Next(movementList.Count) : rnd.Next(movementList.Count - 1);
                    break;
                case MazeGenerationType.MostlyLatest:
                    index = rnd.Next(4) == 1 ? rnd.Next(movementList.Count) : rnd.Next(movementList.Count - 1);
                    break;
                case MazeGenerationType.MostlyRandom:
                    index = rnd.Next(4) != 1 ? rnd.Next(movementList.Count) : rnd.Next(movementList.Count - 1);
                    break;
                default:
                    index = movementList.Count - 1;
                    break;
            }

            return index;
        }

        private static void AdjustWallsAndLadders(MoveDirection dir, MazeCell currentCell, MazeCell nextMazeCell)
        {
            switch (dir)
            {
                case MoveDirection.Top:
                    currentCell.WTop = false;
                    nextMazeCell.WBot = false;
                    break;
                case MoveDirection.Right:
                    currentCell.WRight = false;
                    nextMazeCell.WLeft = false;
                    break;
                case MoveDirection.Bottom:
                    currentCell.WBot = false;
                    nextMazeCell.WTop = false;
                    break;
                case MoveDirection.Left:
                    currentCell.WLeft = false;
                    nextMazeCell.WRight = false;
                    break;
                case MoveDirection.Up:
                    currentCell.Up = true;
                    nextMazeCell.Down = true;
                    break;
                case MoveDirection.Down:
                    currentCell.Down = true;
                    nextMazeCell.Up = true;
                    break;
            }
        }

        private void TryToUnstuck(IList<MazeCell> mList, MazeCell currentCell, int index)
        {
            if (mList.Count == 1) // Temp check for tricky params when maze generation being corrupted
            {
                var first = mList.First();
                if (CheckAroundAvail(first))
                {
                    if (first.Z < CompleteMaze.GetLength(2) - 1)
                    {
                        if (!CompleteMaze[first.X, first.Y, first.Z + 1].Visited)
                        {
                            currentCell.Down = true;
                            var nextCell = CompleteMaze[currentCell.X, currentCell.Y, currentCell.Z + 1];
                            nextCell.DistanceFromStart = currentCell.DistanceFromStart + 1;
                            mList.Add(nextCell);
                            nextCell.Visited = true;
                        }
                    }
                    else if (first.Z > 0)
                    {
                        if (!CompleteMaze[first.X, first.Y, first.Z - 1].Visited)
                        {
                            var nextCell = CompleteMaze[currentCell.X, currentCell.Y, currentCell.Z - 1];
                            nextCell.Down = true;
                            nextCell.DistanceFromStart = currentCell.DistanceFromStart + 1;
                            mList.Add(nextCell);
                            nextCell.Visited = true;
                        }
                    }
                }
                else
                {
                    RemoveCellFromListAndMarkAsDone(mList[index], mList);
                }
            }
            else
            {
                RemoveCellFromListAndMarkAsDone(mList[index], mList);
            }
        }

        private static MoveDirection GetNextMoveDirection(int item, Random rnd, int floorChance)
        {
            MoveDirection dir;
            var normItem = item;
            if (item == 4)
            {
                var upDown = rnd.Next(floorChance + 1);
                if (upDown == floorChance)
                {
                    normItem = 4;
                }
                else if (upDown == floorChance - 1)
                {
                    normItem = 5;
                }
                else
                {
                    return MoveDirection.None;
                }
            }

            switch (normItem)
            {
                case 0:
                    dir = MoveDirection.Top;
                    break;
                case 1:
                    dir = MoveDirection.Right;
                    break;
                case 2:
                    dir = MoveDirection.Bottom;
                    break;
                case 3:
                    dir = MoveDirection.Left;
                    break;
                case 4:
                    dir = MoveDirection.Up;
                    break;
                case 5:
                    dir = MoveDirection.Down;
                    break;
                default: throw new ArgumentException();
            }

            return dir;
        }

        private static void RemoveCellFromListAndMarkAsDone(MazeCell cellToCheck, ICollection<MazeCell> mList)
        {
            cellToCheck.Finished = true;
            cellToCheck.Update();
            mList.Remove(cellToCheck);
        }

        private bool CheckAroundAvail(Coords mc) // TODO: Rewrite to linked cell
        {
            if (mc.X > 0)
                if (!CompleteMaze[mc.X - 1, mc.Y, mc.Z].Visited)
                    return true;
            if (mc.Y > 0)
                if (!CompleteMaze[mc.X, mc.Y - 1, mc.Z].Visited)
                    return true;
            if (mc.Z > 0)
                if (!CompleteMaze[mc.X, mc.Y, mc.Z - 1].Visited)
                    return true;
            if (mc.X < CompleteMaze.GetLength(0) - 1)
                if (!CompleteMaze[mc.X + 1, mc.Y, mc.Z].Visited)
                    return true;
            if (mc.Y < CompleteMaze.GetLength(1) - 1)
                if (!CompleteMaze[mc.X, mc.Y + 1, mc.Z].Visited)
                    return true;
            if (mc.Z < CompleteMaze.GetLength(2) - 1)
                if (!CompleteMaze[mc.X, mc.Y, mc.Z + 1].Visited)
                    return true;

            return false;
        }
        
    }
}