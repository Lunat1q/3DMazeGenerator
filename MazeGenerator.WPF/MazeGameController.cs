using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MazeGenerator.WPF.Annotations;

namespace MazeGenerator.WPF
{
    public class MazeGameController : INotifyPropertyChanged
    {
        private ObservableCollection<MazeFloorViewModel<MazeGameCellViewModel>> _floors;
        private bool _won;
        private bool _easyMode;
        private bool _canBeStarted;
        private int _viewDistance;
        private MazeFloorViewModel<MazeGameCellViewModel> CurrentFloor { get; set; }
        private MazeGameCellViewModel PlayerPos { get; set; }

        public bool Won
        {
            get => _won;
            set
            {
                _won = value;
                OnPropertyChanged();
            }
        }

        public bool EasyMode
        {
            get => _easyMode;
            set
            {
                _easyMode = value;
                OnPropertyChanged();
            }
        }

        public bool CanBeStarted
        {
            get => _canBeStarted;
            set
            {
                _canBeStarted = value;
                OnPropertyChanged();
            }
        }

        public int ViewDistance
        {
            get => _viewDistance;
            set
            {
                if (value == _viewDistance) return;
                _viewDistance = value;
                OnPropertyChanged();
            }
        }

        public MazeGameController(ObservableCollection<MazeFloorViewModel<MazeCellViewModel>> floors)
        {
            Floors = new ObservableCollection<MazeFloorViewModel<MazeGameCellViewModel>>();
            foreach (var floor in floors)
            {
                Floors.Add(floor.ToGameFloor());
            }
        }

        public MazeGameController()
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<MazeFloorViewModel<MazeGameCellViewModel>> Floors
        {
            get => _floors;
            set
            {
                _floors = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Prepare()
        {
            foreach (var f in Floors)
            {
                f.Visible = false;
            }
            try
            {
                PlayerPos = Floors.SelectMany(x => x.FloorData).SelectMany(y => y).First(k => k.Info == "Start");
                PlayerPos.Player = true;
            }
            catch (Exception)
            {
                //meh...
            }

            CanBeStarted = true;
            ViewDistance = 2;
        }

        public void Start()
        {
            try
            {
                // having it as "start" text constant is pure shit-code, but who cares?
                CurrentFloor = Floors.First(x => x.FloorData.SelectMany(y => y).Any(k => k.Player));
                CurrentFloor.Visible = true;
                CalculateVisibility(ViewDistance);
                CanBeStarted = false;
            }
            catch (Exception)
            {
                //meh...
            }
        }
        
        public void CalculateVisibility(int steps)
        {
            if (!EasyMode)
            {
                foreach (var x in CurrentFloor.FloorData.SelectMany(x => x))
                {
                    x.Shadow = true;
                }
            }

            var visibleCells = CurrentFloor.GetVisibleCells(PlayerPos, steps);
            PlayerPos.Shadow = false;
            foreach (var cell in visibleCells)
            {
                cell.Shadow = false;
            }
        }

        public void KeyPressed(Key eKey)
        {
            if (Won) return;
            MazeGameCellViewModel newCell = null;

            switch (eKey)
            {
                case Key.Left:
                    newCell = CurrentFloor.MoveTo(PlayerPos, -1, 0);
                    break;
                case Key.Right:
                    newCell = CurrentFloor.MoveTo(PlayerPos, 1, 0);
                    break;
                case Key.Up:
                    newCell = CurrentFloor.MoveTo(PlayerPos, 0, -1);
                    break;
                case Key.Down:
                    newCell = CurrentFloor.MoveTo(PlayerPos, 0, 1);
                    break;
                case Key.Space:
                    newCell = TryChangeFloor();
                    break;
            }

            if (newCell != null)
            {
                newCell.Player = true;
                PlayerPos.Player = false;
                PlayerPos = newCell;
                CalculateVisibility(ViewDistance);
                if (PlayerPos.Info == "End")
                {
                    Won = true;
                }
            }
        }

        private MazeGameCellViewModel TryChangeFloor()
        {
            if (PlayerPos.Up)
            {
                CurrentFloor.Visible = false;
                CurrentFloor = Floors[PlayerPos.Cell.Z + 1];
                CurrentFloor.Visible = true;
                return CurrentFloor.FloorData[PlayerPos.Cell.Y][PlayerPos.Cell.X];
            }

            if (PlayerPos.Down)
            {
                CurrentFloor.Visible = false;
                CurrentFloor = Floors[PlayerPos.Cell.Z - 1];
                CurrentFloor.Visible = true;
                return CurrentFloor.FloorData[PlayerPos.Cell.Y][PlayerPos.Cell.X];
            }

            return null;
        }
    }
}
