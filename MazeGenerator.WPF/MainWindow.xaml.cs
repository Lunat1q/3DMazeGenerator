using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using MazeGenerator.WPF.Annotations;
using MazeGeneratorCore;

namespace MazeGenerator.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Maze _maze;
        private List<MazeFloorViewModel<MazeCellViewModel>> _floorsViewModels;
        private bool _showDist = true;
        private MazeGenerationType _generationType;

        public MazeGenerationType GenerationType
        {
            get => _generationType;
            set
            {
                _generationType = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            GenerationType = MazeGenerationType.Latest;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(XSize.Text, out var sizeX) ||
                !int.TryParse(YSize.Text, out var sizeY) ||
                !int.TryParse(ZSize.Text, out var sizeZ) ||
                !int.TryParse(Delay.Text, out var delay)) return;

            if (sizeX < 2 || sizeY < 2 || sizeZ < 1) return;

            Floors.Children.Clear();
            _maze = new Maze(sizeX, sizeY, sizeZ, GenerationType) {NextCellDelay = delay};
            _floorsViewModels = new List<MazeFloorViewModel<MazeCellViewModel>>();
            for (var z = 0; z < sizeZ; z++)
            {
                var floorCells = new MazeCellViewModel[sizeX, sizeY];
                for (var i = 0; i < sizeX; i++)
                {
                    for (var j = 0; j < sizeY; j++)
                    {
                        floorCells[i, j] = new MazeCellViewModel(_maze.CompleteMaze[i, j, z], true);
                    }
                }

                var floorVm = new MazeFloorViewModel<MazeCellViewModel>(floorCells);
                _floorsViewModels.Add(floorVm);
                var floor = new MazeFloorControl(floorVm);
                Floors.Children.Add(floor);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _maze.MazeGenerated += MazeGenerated;
            _maze.GenerateMaze();
            
        }

        private void MazeGenerated(object sender, EventArgs e)
        {
            foreach (var floor in _floorsViewModels)
            {
                floor.SwitchDebug(false);
            }

            var allCells = _floorsViewModels.SelectMany(x => x.FloorData).SelectMany(x => x).ToList();
            var maxDist = allCells.Max(x => x.DistanceFromStart);
            foreach (var cell in allCells.Where(x => x.DistanceFromStart == maxDist))
            {
                cell.Info = "End";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _showDist = !_showDist;
            var allCells = _floorsViewModels.SelectMany(x => x.FloorData).SelectMany(x => x).ToList();
            foreach (var cell in allCells)
            {
                cell.ShowDistance = _showDist;
            }
        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var allCells = _floorsViewModels.SelectMany(x => x.FloorData).SelectMany(x => x).ToList();
            var maxDist = allCells.Max(x => x.DistanceFromStart);
            var cell = allCells.First(x => x.DistanceFromStart == maxDist);

            while (cell.DistanceFromStart > 1)
            {
                var cell1 = cell;
                var closeDistanceCells = allCells
                    .Where(x => x.DistanceFromStart == cell1.DistanceFromStart - 1);
                var nextCell = closeDistanceCells
                    .First(x => CanMoveToCell(cell.Cell, x.Cell));
                 cell = nextCell;
                 cell.Info = "!";
                 await Task.Delay(1);
            }
        }

        private bool CanMoveToCell(MazeCell fromCell, Coords toCell)
        {
            if (fromCell.X - toCell.X == -1 && fromCell.Y == toCell.Y && fromCell.Z == toCell.Z &&
                !fromCell.WRight) return true;
            if (fromCell.X - toCell.X == 1 && fromCell.Y == toCell.Y && fromCell.Z == toCell.Z &&
                !fromCell.WLeft) return true;
            if (fromCell.X == toCell.X && fromCell.Y - toCell.Y == 1 && fromCell.Z == toCell.Z &&
                !fromCell.WTop) return true;
            if (fromCell.X == toCell.X && fromCell.Y - toCell.Y == -1 && fromCell.Z == toCell.Z &&
                !fromCell.WBot) return true;
            if (fromCell.X == toCell.X && fromCell.Y == toCell.Y && fromCell.Z - toCell.Z == -1 &&
                fromCell.Up) return true;
            if (fromCell.X == toCell.X && fromCell.Y == toCell.Y && fromCell.Z - toCell.Z == 1 &&
                fromCell.Down) return true;

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var mg = new MazeGameWindow(new MazeGameController(new ObservableCollection<MazeFloorViewModel<MazeCellViewModel>>(_floorsViewModels)));
            mg.Show();
        }
    }
}
