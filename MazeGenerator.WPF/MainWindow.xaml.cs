using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MazeGeneratorCore;

namespace MazeGenerator.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Maze _maze;
        private List<MazeFloorViewModel> _floorsViewModels;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(XSize.Text, out var sizeX) ||
                !int.TryParse(YSize.Text, out var sizeY) ||
                !int.TryParse(ZSize.Text, out var sizeZ) ||
                !int.TryParse(Delay.Text, out var delay)) return;

            if (sizeX < 2 || sizeY < 2 || sizeZ < 1) return;

            Floors.Children.Clear();
            _maze = new Maze(sizeX, sizeY, sizeZ) {NextCellDelay = delay};
            _floorsViewModels = new List<MazeFloorViewModel>();
            var floorCells = new MazeCellViewModel[sizeX, sizeY];
            for (var z = 0; z < sizeZ; z++)
            {
                for (var i = 0; i < sizeX; i++)
                {
                    for (var j = 0; j < sizeY; j++)
                    {
                        floorCells[i, j] = new MazeCellViewModel(_maze.CompleteMaze[i, j, z], true);
                    }
                }

                var floorVm = new MazeFloorViewModel(floorCells);
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
    }
}
