
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using ImageShrinker.Helper;
using System.Linq;

namespace ImageShrinker.ViewModels
{
    public class ImageShrinkerViewModel : ViewModel
    {
        private Size _desiredSizeStat;
        private readonly Dictionary<string, string> _filenames;
        private ImageThumbViewModel _selectedThumb;

        public ImageShrinkerViewModel()
        {
            ImageThumbs = new ObservableCollectionEx<ImageThumbViewModel>();
            Scale = 100;
            Quality = 90;
            ArchiveName = "BilderArchiv";
            _filenames = new Dictionary<string, string>();
            _selectedThumb = null;
        }

        public Size DesiredSize { get { return new Size(_desiredSizeStat.Width * Scale * 0.01, _desiredSizeStat.Height * Scale * 0.01); } }

        public void Select(int index)
        {
            if (index < ImageThumbs.Count && index != -1)
            {
                _selectedThumb = ImageThumbs[index];
                PreViewImage = _selectedThumb.AbsolutePath;
            }
        }
        public void SelectAllImages(bool select)
        {
            foreach (ImageThumbViewModel thumb in ImageThumbs)
            {
                thumb.IsSelected = select;
            }
        }

        public int SelectedImageCount 
        {
            get { return ImageThumbs.Count(t => t.IsSelected); }
        }

        public double SelectedFileSizeCount
        {
            get 
            {
                return Math.Round(ImageThumbs.Where(i => i.IsSelected).Sum(i => i.OriginalFileSize), 1);
            }
        }

        public double CompressedSize
        {
            get 
            {
                return Math.Round(ImageThumbs.Where(i => i.IsSelected).Sum(i => i.GetFileSizeScaled((int)DesiredSize.Width,Quality)), 1);
            }
        }

        public void NotifySelectedImageCountChanged()
        {
            NotifyPropertyChanged("SelectedImageCount");
            NotifyPropertyChanged("SelectedFileSizeCount");
        }

        public void CalculateStatistics()
        {
            _desiredSizeStat = new Size(0, 0);

            foreach (ImageThumbViewModel thumb in ImageThumbs)
            {
                if (thumb.Width > _desiredSizeStat.Width)
                {
                    _desiredSizeStat = new Size(thumb.Width, thumb.Height);
                }
            }
        }
        public void UpdateStatistics()
        {
            double factor = Scale / 100.0;
            AverageSize = string.Format("{0} x {1}", (int)(_desiredSizeStat.Width * factor), (int)(_desiredSizeStat.Height * factor));
        }
        
        private string GetUniqueFileName(string filename)
        {
            int currentNumber = 1;
            string newfile = string.Format(filename + "({0})", currentNumber);
            while (_filenames.ContainsKey(newfile))
            {
                currentNumber++;
                newfile = string.Format(filename + "({0})", currentNumber);
            }
            return newfile;
        }
        public void AddImage(string url)
        {
            ImageThumbViewModel thumb = new ImageThumbViewModel(url, this);

            string filename = Path.GetFileNameWithoutExtension(url);
            if (!string.IsNullOrEmpty(filename))
            {
                if (_filenames.ContainsKey(filename))
                {
                    filename = GetUniqueFileName(filename);
                    thumb.FileName = filename + ".jpg";
                }
                else
                    thumb.FileName = filename + ".jpg";
                _filenames.Add(filename, filename);

                ImageThumbs.Add(thumb);    
            }
        }

        public ObservableCollectionEx<ImageThumbViewModel> ImageThumbs { get; set; }
        private string _preViewImage;
        public string PreViewImage
        {
            get 
            {
                return _preViewImage;
            }
            set
            {
                if (_preViewImage != value)
                {
                    _preViewImage = value;
                    NotifyPropertyChanged("PreViewImage");
                    NotifyPropertyChanged("PreViewRotation");
                }
            }
        }

        private double _scale;
        public double Scale 
        {
            get { return _scale; }
            set 
            { 
                _scale = value; 
                UpdateStatistics();
                NotifyPropertyChanged("Scale");
            }
        }

        private int _quality;
        public int Quality
        {
            get { return _quality; }
            set
            {
                _quality = value;
                NotifyPropertyChanged("Quality");
            }
        }

        public double PreViewRotation
        {
            get { return _selectedThumb != null ? _selectedThumb.Rotation : 0; }
            set 
            {
                if (_selectedThumb != null)
                {
                    _selectedThumb.Rotation = value;
                    NotifyPropertyChanged("PreViewRotation");
                }
            }
        }

        private string _averageSize;
        public string AverageSize { get { return _averageSize; } set { _averageSize = value; NotifyPropertyChanged("AverageSize"); } }

        private string _archiveName;
        public string ArchiveName { get { return _archiveName; } set { _archiveName = value; NotifyPropertyChanged("ArchiveName"); } }
    }
}
