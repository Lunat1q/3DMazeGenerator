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

        public MazeCell[,,] CompleteMaze { get; set; }
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

            var mList = new List<MazeCell> {firstCell};

            MoveDirection dir = 0;

            //TEST - making UP/Down ways much less to appear (attempt at least) :P
            var floorChanceInit = (maxX * maxY * maxZ) / 50 > 0 ? (maxX * maxY * maxZ) / 50 : 1;
            var floorChance = floorChanceInit;
            var floorSwitch = 1;
            do
            {
                int index;
                switch (_genType)
                {
                    case MazeGenerationType.Latest:
                        //Latest
                        index = mList.Count - 1;
                        break;
                    case MazeGenerationType.Random:
                        //Random!
                        index = rnd.Next(mList.Count);
                        break;
                    case MazeGenerationType.HalfHalf:
                        index = rnd.Next(2) == 1 ? rnd.Next(mList.Count) : rnd.Next(mList.Count - 1);
                        break;
                    case MazeGenerationType.MostlyLatest:
                        index = rnd.Next(4) == 1 ? rnd.Next(mList.Count) : rnd.Next(mList.Count - 1);
                        break;
                    case MazeGenerationType.MostlyRandom:
                        index = rnd.Next(4) != 1 ? rnd.Next(mList.Count) : rnd.Next(mList.Count - 1);
                        break;
                    default:
                        index = mList.Count - 1;
                        break;
                }

                var currentCell = mList[index];
                var randomNumbers = Enumerable.Range(0, 5).OrderBy(x => rnd.Next()).Take(5);
                var found = false;
                var nextX = currentCell.X;
                var nextY = currentCell.Y;
                var nextZ = currentCell.Z;
                foreach (var item in randomNumbers)
                {
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
                            continue;
                        }
                    }

                    switch (normItem)
                    {
                        case 0:
                            nextY = currentCell.Y - 1;
                            nextX = currentCell.X;
                            nextZ = currentCell.Z;
                            dir = MoveDirection.Top;
                            break;
                        case 1:
                            nextY = currentCell.Y;
                            nextX = currentCell.X + 1;
                            nextZ = currentCell.Z;
                            dir = MoveDirection.Right;
                            break;
                        case 2:
                            nextY = currentCell.Y + 1;
                            nextX = currentCell.X;
                            nextZ = currentCell.Z;

                            dir = MoveDirection.Bottom;
                            break;
                        case 3:
                            nextY = currentCell.Y;
                            nextX = currentCell.X - 1;
                            nextZ = currentCell.Z;
                            dir = MoveDirection.Left;
                            break;
                        case 4:
                            nextY = currentCell.Y;
                            nextX = currentCell.X;
                            nextZ = currentCell.Z + 1;
                            dir = MoveDirection.Up;
                            break;
                        case 5:
                            nextY = currentCell.Y;
                            nextX = currentCell.X;
                            nextZ = currentCell.Z - 1;
                            dir = MoveDirection.Down;
                            break;
                    }

                    found = true;

                    if (nextY >= 0 && nextX >= 0 && nextZ >= 0 && nextX < maxX && nextY < maxY && nextZ < maxZ)
                    {
                        if (!CompleteMaze[nextX, nextY, nextZ].Visited) // Double Floor check
                            if ((nextZ - currentCell.Z > 0 && CompleteMaze[nextX, nextY, nextZ].Floor) ||
                                (nextZ - currentCell.Z < 0 && currentCell.Floor) || nextZ - currentCell.Z == 0)
                            {
                                if (nextZ - currentCell.Z > 0 && nextZ > 1)
                                {
                                    if (CompleteMaze[nextX, nextY, nextZ - 2].Floor)
                                        break;
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
                    if (mList.Count == 1) // Temp check for tricky params when maze generation being corrupted
                    {
                        if (CheckAroundAvail(mList[0]))
                        {
                            if (mList[0].Z < CompleteMaze.GetLength(2) - 1)
                            {
                                if (!CompleteMaze[mList[0].X, mList[0].Y, mList[0].Z + 1].Visited)
                                {
                                    currentCell.Floor = false;
                                    var nextCell = CompleteMaze[currentCell.X, currentCell.Y, currentCell.Z + 1];
                                    nextCell.DistanceFromStart = currentCell.DistanceFromStart + 1;
                                    mList.Add(nextCell);
                                    nextCell.Visited = true;
                                }
                            }
                            else if (mList[0].Z > 0)
                            {
                                if (!CompleteMaze[mList[0].X, mList[0].Y, mList[0].Z - 1].Visited)
                                {
                                    var nextCell = CompleteMaze[currentCell.X, currentCell.Y, currentCell.Z - 1];
                                    nextCell.Floor = false;
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
                else
                {
                    for (var m = mList.Count - 1;
                        m > 0;
                        m--) // Test Algorithm, should increase passage length for 1-4 creation variants
                    {
                        var cellToCheck = mList[m];
                        if (!CheckAroundAvail(cellToCheck))
                        {
                            RemoveCellFromListAndMarkAsDone(cellToCheck, mList);
                        }
                    }

                    if (dir == MoveDirection.Up || dir == MoveDirection.Down)
                    {
                        floorSwitch++;
                        floorChance *= floorSwitch;
                    }
                    else
                    {
                        if (floorChance > 100) floorChance = (int) Math.Sqrt(floorChance);
                        else if (floorChance > floorChanceInit * 2)
                            floorChance = floorChance - 1 > 0 ? floorChance - 1 : 3;
                    }

                    var nextCell = CompleteMaze[nextX, nextY, nextZ];
                    switch (dir)
                    {
                        case MoveDirection.Top:
                            currentCell.WTop = false;
                            nextCell.WBot = false;
                            break;
                        case MoveDirection.Right:
                            currentCell.WRight = false;
                            nextCell.WLeft = false;
                            break;
                        case MoveDirection.Bottom:
                            currentCell.WBot = false;
                            nextCell.WTop = false;
                            break;
                        case MoveDirection.Left:
                            currentCell.WLeft = false;
                            nextCell.WRight = false;
                            break;
                        case MoveDirection.Up:
                            currentCell.Up = true;
                            nextCell.Down = true;
                            nextCell.Floor = false;
                            break;
                        case MoveDirection.Down:
                            currentCell.Down = true;
                            nextCell.Up = true;
                            currentCell.Floor = false;
                            break;
                    }

                    nextCell.Visited = true;
                    nextCell.DistanceFromStart = currentCell.DistanceFromStart + 1;
                    mList.Add(nextCell);
                    nextCell.Update();
                }

                currentCell.Update();
                await Task.Delay(NextCellDelay);
            } while (mList.Count > 0);

            MazeGenerated?.Invoke(this, EventArgs.Empty);
        }

        private static void RemoveCellFromListAndMarkAsDone(MazeCell cellToCheck, List<MazeCell> mList)
        {
            cellToCheck.Finished = true;
            cellToCheck.Update();
            mList.Remove(cellToCheck);
        }

        private bool CheckAroundAvail(MazeCell mc)
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