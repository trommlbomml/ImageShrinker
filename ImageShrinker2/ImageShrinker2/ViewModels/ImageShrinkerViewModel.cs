
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using ImageShrinker2.Framework;
using ImageShrinker2.Model;
using Ionic.Zip;

namespace ImageShrinker2.ViewModels
{
    class ImageShrinkerViewModel : ViewModel
    {
        private readonly ObservableCollection<ImageViewModel> _images;
        private ImageViewModel _selectedImage;
        private string _archiveName;
        private double _scale;
        private int _maxWidth;
        private int _maxHeight;

        public ReadOnlyObservableCollection<ImageViewModel> Images
        { get { return new ReadOnlyObservableCollection<ImageViewModel>(_images); } }

        public ViewModelCommand AddFilesCommand { get; private set; }
        public ViewModelCommand AddFromFolderCommand { get; private set; }
        public ViewModelCommand SaveToFolderCommand { get; private set; }
        public ViewModelCommand PackToFolderCommand { get; private set; }

        public ImageShrinkerViewModel()
        {
            _images = new ObservableCollection<ImageViewModel>();
            AddFilesCommand = new ViewModelCommand(AddFilesCommandExecuted);
            AddFromFolderCommand = new ViewModelCommand(AddFromFolderCommandExecuted);
            SaveToFolderCommand = new ViewModelCommand(SaveToFolderCommandExecuted);
            PackToFolderCommand = new ViewModelCommand(PackToFolderCommandExecuted);

            PropertyChanged += OnViewModelPropertyChanged;
            Scale = 100.0;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Scale")
            {
                OnPropertyChanged("DesiredWidth");
                OnPropertyChanged("DesiredHeight");
            }
        }

        private void UpdateDesiredSize()
        {
            _maxWidth = 0;
            _maxHeight = 0;
            foreach (ImageViewModel imageViewModel in _images)
            {
                if (imageViewModel.Width > _maxWidth)
                {
                    _maxWidth = imageViewModel.Width;
                    _maxHeight = imageViewModel.Height;
                }
            }
            OnPropertyChanged("DesiredWidth");
            OnPropertyChanged("DesiredHeight");
        }

        private void PackToFolderCommandExecuted()
        {
            string path;
            if (!ViewService.ChooseFolderDialog(out path))
                return;

            string tempPath = Path.Combine(Path.GetTempPath(), "ImageShrinker", ArchiveName);
            
            try
            {
                Directory.CreateDirectory(tempPath);
                SaveTo(tempPath);
                using(ZipFile zipFile = new ZipFile())
                {
                    foreach (string file in Directory.GetFiles(tempPath))
                    {
                        zipFile.AddFile(file, ArchiveName);
                    }
                    zipFile.Save(Path.Combine(path, ArchiveName + ".zip"));
                }
            }
            finally
            {
                if (Directory.Exists(tempPath))
                {
                    foreach (string file in Directory.GetFiles(tempPath))
                        File.Delete(file);
                    Directory.Delete(tempPath);
                }
            }
        }

        private void SaveToFolderCommandExecuted()
        {
            string path;
            if (!ViewService.ChooseFolderDialog(out path))
                return;

            string targetPath = Path.Combine(path, ArchiveName);
            Directory.CreateDirectory(targetPath);
            SaveTo(targetPath);
        }

        private void SaveTo(string path)
        {
            foreach (ImageViewModel imageViewModel in _images)
            {
                ImageModel.SaveScaled(imageViewModel, path, DesiredWidth, DesiredHeight, 100);
            }
        }

        private void AddFromFolderCommandExecuted()
        {
            string path;
            if (ViewService.ChooseFolderDialog(out path))
            {
                AddFromFolderInternal(path);
                UpdateDesiredSize();
            }
        }

        private void AddFromFolderInternal(string path)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                if (new FileInfo(file).Attributes == FileAttributes.Directory)
                {
                    AddFromFolderInternal(file);
                }
                else
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg")
                    {
                        _images.Add(ImageModel.CreateFromFile(file));
                    }
                }
            }
        }

        private void AddFilesCommandExecuted()
        {
            string[] files;
            if(ViewService.ChooseFilesDialog(out files))
            {
                foreach (string file in files)
                {
                    _images.Add(ImageModel.CreateFromFile(file));
                }
            }
        }

        public void AddImage(ImageViewModel imageViewModel)
        {
            _images.Add(imageViewModel);
        }

        public ImageViewModel SelectedImage
        {
            get { return _selectedImage; }
            set { SetBackingField("SelectedImage", ref _selectedImage, value); }
        }

        public string ArchiveName
        {
            get { return _archiveName; }
            set { SetBackingField("ArchiveName", ref _archiveName, value); }
        }

        public double Scale
        {
            get { return _scale; }
            set { SetBackingField("Scale", ref _scale, value ); }
        }

        public int DesiredWidth { get { return (int) (_maxWidth * Scale * 0.01); } }
        public int DesiredHeight { get { return (int)(_maxHeight * Scale * 0.01); } }
    }
}
