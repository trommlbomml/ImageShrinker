
using ImageShrinker2.Framework;

namespace ImageShrinker2.ViewModels
{
    class ImageViewModel : ViewModel
    {
        private string _name;
        private int _width;
        private int _height;
        private string _path;
        private bool _isSelected;
        private double _rotation;

        public ViewModelCommand RotateCcwCommand { get; private set; }
        public ViewModelCommand RotateCwCommand { get; private set; }

        public ImageViewModel()
        {
            RotateCcwCommand = new ViewModelCommand(() => Rotation += 90);
            RotateCwCommand = new ViewModelCommand(() => Rotation -= 90);
        }

        public string Name
        {
            get { return _name; }
            set { SetBackingField("Name", ref _name, value); }
        }

        public string Path
        {
            get { return _path; }
            set { SetBackingField("Path", ref _path, value); }
        }

        public int Width
        {
            get { return _width; }
            set { SetBackingField("Width", ref _width, value); }
        }

        public int Height
        {
            get { return _height; }
            set { SetBackingField("Height", ref _height, value); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetBackingField("IsSelected", ref _isSelected, value); }
        }

        public double Rotation
        {
            get { return _rotation; }
            set { SetBackingField("Rotation", ref _rotation, value); }
        }
    }
}
