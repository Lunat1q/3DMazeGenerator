using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MazeGenerator.WPF.Annotations;
using MazeGeneratorCore;

namespace MazeGenerator.WPF
{
    internal class MazeCellViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly MazeCell _cell;
        private bool _debug;
        private bool _showDistance;

        public MazeCellViewModel(MazeCell cell, bool debug = false)
        {
            _cell = cell;
            _cell.CellChanged += CellOnCellChanged;
            Debug = debug;
        }

        private void CellOnCellChanged(object sender, EventArgs e)
        {
            Update();
        }

        public void Update()
        {
            OnPropertyChanged(nameof(WBot));
            OnPropertyChanged(nameof(WLeft));
            OnPropertyChanged(nameof(WRight));
            OnPropertyChanged(nameof(WTop));
            OnPropertyChanged(nameof(Floor));
            OnPropertyChanged(nameof(Visited));
            OnPropertyChanged(nameof(Finished));
            OnPropertyChanged(nameof(Up));
            OnPropertyChanged(nameof(Down));
            OnPropertyChanged(nameof(Closed));
            OnPropertyChanged(nameof(Info));
            OnPropertyChanged(nameof(DistanceFromStart));
            OnPropertyChanged(nameof(Coords));
        }

        public bool Down
        {
            get => _cell.Down;
            set { _cell.Down = value;
                OnPropertyChanged();
            }
        }

        public bool Up
        {
            get => _cell.Up;
            set { _cell.Up = value;
                OnPropertyChanged();
            }
        }

        public MazeCellViewModel()
        {
            
        }

        public bool WTop
        {
            get => _cell.WTop;
            set { _cell.WTop = value;
                OnPropertyChanged();
            }
        }

        public bool WLeft
        {
            get => _cell.WLeft;
            set { _cell.WLeft = value;
                OnPropertyChanged();
            }
        }

        public bool WRight
        {
            get => _cell.WRight;
            set { _cell.WRight = value;
                OnPropertyChanged();
            }
        }

        public bool WBot
        {
            get => _cell.WBot;
            set { _cell.WBot = value;
                OnPropertyChanged();
            }
        }

        public bool Floor
        {
            get => _cell.Floor;
            set { _cell.Floor = value;
                OnPropertyChanged();
            }
        }

        public bool Visited
        {
            get => _cell.Visited && !Finished;
            set { _cell.Visited = value;
                OnPropertyChanged();
            }
        }

        public string Info
        {
            get => _cell.Info;
            set { _cell.Info = value;
                OnPropertyChanged();
            }
        }

        public bool Closed => !Visited && !Finished;

        public bool Finished
        {
            get => _cell.Finished;
            set { _cell.Finished = value;
                OnPropertyChanged();
            }
        }

        public bool Debug
        {
            get => _debug;
            set
            {
                _debug = value;
                OnPropertyChanged();
            }
        }

        public string Coords => $"{_cell.X}:{_cell.Y}:{_cell.Z}";

        public bool ShowDistance
        {
            get => _showDistance;
            set
            {
                _showDistance = value;
                OnPropertyChanged();
            }
        }

        public int DistanceFromStart
        {
            get => _cell.DistanceFromStart;
            set { _cell.DistanceFromStart = value;
                OnPropertyChanged();
            }
        }
    }
}
