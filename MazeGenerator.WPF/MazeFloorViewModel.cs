using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MazeGenerator.WPF.Annotations;
using MazeGenerator.WPF.Helpers;

namespace MazeGenerator.WPF
{
    public class MazeFloorViewModel<T> : INotifyPropertyChanged where T : MazeCellViewModel
    {
        private ObservableCollection<ObservableCollection<T>> _floorData;
        private bool _visible = true;
        public event PropertyChangedEventHandler PropertyChanged;
        private T[,] OriginalData { get; set; }
        private T[,] GameData { get; set; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<ObservableCollection<T>> FloorData
        {
            get => _floorData;
            private set
            {
                _floorData = value;
                OnPropertyChanged();
            }
        }

        public MazeFloorViewModel(T[,] cellData)
        {
            OriginalData = cellData;
            FloorData = new ObservableCollection<ObservableCollection<T>>();
            for (var i = 0; i < cellData.GetLength(1); i++)
            {
                var innerList = new ObservableCollection<T>();
                for (var j = 0; j < cellData.GetLength(0); j++)
                {
                    innerList.Add(cellData[j, i]);
                }
                FloorData.Add(innerList);
            }
        }

        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                OnPropertyChanged();
            }
        }

        public MazeFloorViewModel()
        {
            //throw new System.NotImplementedException();
        }

        public void SwitchDebug(bool debug)
        {
            foreach (var cell in FloorData.SelectMany(x => x))
            {
                cell.Debug = debug;
                cell.ShowDistance = !debug;
            }
        }

        public MazeFloorViewModel<MazeGameCellViewModel> ToGameFloor()
        {
            var gameCells = new MazeGameCellViewModel[OriginalData.GetLength(0), OriginalData.GetLength(1)];
            for (var i = 0; i < OriginalData.GetLength(0); i++)
            {
                for (var j = 0; j < OriginalData.GetLength(1); j++)
                {
                    gameCells[i, j] = new MazeGameCellViewModel(OriginalData[i, j].Cell);
                }
            }
            return new MazeFloorViewModel<MazeGameCellViewModel>(gameCells);
        }

        internal IEnumerable<T> GetVisibleCells(T cell, int sightDist = 2)
        {
            var result = new List<T>();

            var visibleCellsAround = GetCellsAround(cell).Where(cell.CanSeeCell).ToList();
            result.AddRange(visibleCellsAround);
            foreach (var visibleCell in visibleCellsAround)
            {
                var distLeft = sightDist - 1;
                var dx = visibleCell.Cell.X - cell.Cell.X;
                var dy = visibleCell.Cell.Y - cell.Cell.Y;
                var nextCell = visibleCell;
                while (distLeft > 0)
                {
                    var potentialCell = TryGetCellFromCoords(nextCell.Cell.X + dx, nextCell.Cell.Y + dy);
                    if (potentialCell != null && nextCell.CanSeeCell(potentialCell))
                    {
                        result.Add(potentialCell);
                        nextCell = potentialCell;
                    }
                    else
                    {
                        break;
                    }
                    distLeft--;
                }
            }
            return result;
        }

        private T TryGetCellFromCoords(int x, int y)
        {
            if (OriginalData.IndexesAreInRange(x, y))
            {
                return OriginalData[x, y];
            }

            return null;
        }

        private IEnumerable<T> GetCellsAround(T cell)
        {
            var result = new List<T>();

            var row = cell.Cell.Y;
            var col = cell.Cell.X;

            if (col > 0)
            {
                result.Add(OriginalData[col - 1, row]);
            }
            if (row > 0)
            {
                result.Add(OriginalData[col, row - 1]);
            }
            if (row < OriginalData.GetUpperBound(1))
            {
                result.Add(OriginalData[col, row + 1]);
            }
            if (col < OriginalData.GetUpperBound(0))
            {
                result.Add(OriginalData[col + 1, row]);
            }

            return result;
        }

        public T MoveTo(T fromCell, int dx, int dy)
        {
            var x = fromCell.Cell.X + dx;
            var y = fromCell.Cell.Y + dy;
            if (x >= 0 && y >= 0 && y < OriginalData.GetLength(1) && x < OriginalData.GetLength(0))
            {
                var possibleNewCell = OriginalData[x, y];
                if (fromCell.CanMoveToCell(possibleNewCell))
                {
                    return possibleNewCell;
                }
            }

            return null;
        }
    }
}
