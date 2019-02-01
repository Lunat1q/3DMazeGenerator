using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MazeGeneratorCore
{
    public sealed class Maze
    {
        private readonly int _xSize;
        private readonly int _ySize;
        private readonly int _zSize;
        private readonly MazeGenerationType _genType;

        public MazeCell[,,] CompleteMaze { get; set; } // TODO: Rewrite to linked cell to allow any configuration of the grid.
        public int NextCellDelay { get; set; } = 0;

        public EventHandler MazeGenerated;

        public Maze(int xSize, int ySize, int zSize, MazeGenerationType genType = 0)
        {
            _xSize = xSize;
            _ySize = ySize;
            _zSize = zSize;
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
            var maxX = _xSize;
            var maxY = _ySize;
            var maxZ = _zSize;

            var rnd = randomGenerator ?? new Random();
            //Random Generation Start Point attempt if first cell is not set;
            var firstCell = startCell ?? CompleteMaze[rnd.Next(maxX), rnd.Next(maxY), rnd.Next(maxZ)];

            firstCell.Info = "Start";
            firstCell.Visited = true;

            var movementList = new List<MazeCell> {firstCell};


            //TEST - making UP/Down ways much less to appear (attempt at least) :P
            var floorChanceInit = maxX * maxY * maxZ / 50 > 0 ? maxX * maxY * maxZ / 50 : 1;
            var floorChance = floorChanceInit;
            var floorSwitch = 1;
            do
            {
                var index = GetNextCellIndexToMoveFrom(movementList, rnd);

                var currentCell = movementList[index];
                var possibleMoves = Enumerable.Range(0, 5).OrderBy(x => rnd.Next()).Take(5);
                var found = false;
                
                MoveDirection dir = 0;
                MazeCell nextMazeCell = null;
                foreach (var item in possibleMoves)
                {
                    dir = GetNextMoveDirection(item, rnd, floorChance);
                    if (dir == MoveDirection.None) continue;

                    var nextCoords = currentCell.GetCoords().AdjustCoords(dir);

                    found = true;

                    if (nextCoords.ValidateCoordinates(maxX, maxY, maxZ))
                    {
                        nextMazeCell = CompleteMaze[nextCoords.X, nextCoords.Y, nextCoords.Z];
                        if (!nextMazeCell.Visited) // Double Floor check
                            if (nextCoords.Z - currentCell.Z > 0 && !nextMazeCell.Down ||
                                nextCoords.Z - currentCell.Z < 0 && !currentCell.Down || 
                                nextCoords.Z - currentCell.Z == 0)
                            {
                                if (nextCoords.Z - currentCell.Z > 0 && nextCoords.Z > 1)
                                {
                                    if (!CompleteMaze[nextCoords.X, nextCoords.Y, nextCoords.Z - 2].Down) break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                    }

                    found = false;
                }

                if (!found)
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

                    AdjustCellsWallsAndLadders(dir, currentCell, nextMazeCell);

                    nextMazeCell.Visited = true;
                    nextMazeCell.DistanceFromStart = currentCell.DistanceFromStart + 1;
                    movementList.Add(nextMazeCell);
                    nextMazeCell.Update();
                }

                currentCell.Update();
                await Task.Delay(NextCellDelay);
            } while (movementList.Count > 0);

            MazeGenerated?.Invoke(this, EventArgs.Empty);
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

        private int GetNextCellIndexToMoveFrom(List<MazeCell> movementList, Random rnd)
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

        private static void AdjustCellsWallsAndLadders(MoveDirection dir, MazeCell currentCell, MazeCell nextMazeCell)
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