using MazeGeneratorCore;

namespace MazeGenerator.WPF
{
    public class MazeGameCellViewModel : MazeCellViewModel
    {
        private bool _player;
        private bool _shadow = true;

        public MazeGameCellViewModel(MazeCell cell) : base(cell, false)
        {
        }

        public bool Player
        {
            get => _player;
            set
            {
                _player = value;
                OnPropertyChanged();
            }
        }

        public bool Shadow
        {
            get => _shadow;
            set
            {
                _shadow = value;
                OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            return base.Cell.ToString();
        }
    }
}