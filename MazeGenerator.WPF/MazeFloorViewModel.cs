using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MazeGenerator.WPF.Annotations;

namespace MazeGenerator.WPF
{
    internal class MazeFloorViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ObservableCollection<MazeCellViewModel>> _floorData;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<ObservableCollection<MazeCellViewModel>> FloorData
        {
            get => _floorData;
            private set
            {
                OnPropertyChanged();
                _floorData = value;
            }
        }

        public MazeFloorViewModel(MazeCellViewModel[,] cellData)
        {
            FloorData = new ObservableCollection<ObservableCollection<MazeCellViewModel>>();
            for (var i = 0; i < cellData.GetLength(1); i++)
            {
                var innerList = new ObservableCollection<MazeCellViewModel>();
                for (var j = 0; j < cellData.GetLength(0); j++)
                {
                    innerList.Add(cellData[j, i]);
                }
                FloorData.Add(innerList);
            }
        }

        public MazeFloorViewModel()
        {
            throw new System.NotImplementedException();
        }

        public void SwitchDebug(bool debug)
        {
            foreach (var cell in FloorData.SelectMany(x => x))
            {
                cell.Debug = debug;
                cell.ShowDistance = !debug;
            }
        }
    }
}
