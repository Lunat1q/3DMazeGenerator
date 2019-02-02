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

        public MazeCell Cell { get; }
        private bool _debug;
        private bool _showDistance;

        public MazeCellViewModel(MazeCell cell, bool debug = false)
        {
            Cell = cell;
            Cell.CellChanged += CellOnCellChanged;
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
            OnPropertyChanged(nameof(Visited));
            OnPropertyChanged(nameof(Finished));
            OnPropertyChanged(nameof(Up));
            OnPropertyChanged(nameof(Down));
            OnPropertyChanged(nameof(Closed));
            OnPropertyChanged(nameof(Info));
            OnPropertyChanged(nameof(DistanceFromStart));
            OnPropertyChanged(nameof(Coords));
            OnPropertyChanged(nameof(HaveInfo));
        }

        public bool Down
        {
            get => Cell.Down;
            set { Cell.Down = value;
                OnPropertyChanged();
            }
        }

        public bool HaveInfo => !string.IsNullOrWhiteSpace(this.Info);

        public bool Up
        {
            get => Cell.Up;
            set { Cell.Up = value;
                OnPropertyChanged();
            }
        }

        public MazeCellViewModel()
        {
            
        }

        public bool WTop
        {
            get => Cell.WTop;
            set { Cell.WTop = value;
                OnPropertyChanged();
            }
        }

        public bool WLeft
        {
            get => Cell.WLeft;
            set { Cell.WLeft = value;
                OnPropertyChanged();
            }
        }

        public bool WRight
        {
            get => Cell.WRight;
            set { Cell.WRight = value;
                OnPropertyChanged();
            }
        }

        public bool WBot
        {
            get => Cell.WBot;
            set { Cell.WBot = value;
                OnPropertyChanged();
            }
        }

        public bool Visited
        {
            get => Cell.Visited && !Finished;
            set { Cell.Visited = value;
                OnPropertyChanged();
            }
        }

        public string Info
        {
            get => Cell.Info;
            set { Cell.Info = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HaveInfo));
            }
        }

        public bool Closed => !Visited && !Finished;

        public bool Finished
        {
            get => Cell.Finished;
            set { Cell.Finished = value;
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

        public string Coords => $"{Cell.X}:{Cell.Y}:{Cell.Z}";

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
            get => Cell.DistanceFromStart;
            set { Cell.DistanceFromStart = value;
                OnPropertyChanged();
            }
        }
    }
}
