
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;
using ImageShrinker2.Framework;
using ImageShrinker2.Jobs;
using ImageShrinker2.Model;
using ImageShrinker2.Windows;
using System.Linq;

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
        private long _quality;
        private double _compressedSize;
        private DispatcherTimer _timer;
        private bool _imageDataChangedForCalculation;
        private StatusbarViewModel _statusbar;
        private EstimateThread _estimate;
        private bool _hasImages;

        public ReadOnlyObservableCollection<ImageViewModel> Images
        { get { return new ReadOnlyObservableCollection<ImageViewModel>(_images); } }

        public ViewModelCommand AddFilesCommand { get; private set; }
        public ViewModelCommand AddFromFolderCommand { get; private set; }
        public ViewModelCommand SaveToFolderCommand { get; private set; }
        public ViewModelCommand PackToFolderCommand { get; private set; }
        public ViewModelCommand ShowInfoCommand { get; private set; }
        public ViewModelCommand SendPerMailCommand { get; private set; }
        public ViewModelCommand UnifyImageNamesCommand { get; private set; }

        public ImageShrinkerViewModel()
        {
            _timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 1)};
            _timer.Tick += TimerOnTick;

            _images = new ObservableCollection<ImageViewModel>();
            AddFilesCommand = new ViewModelCommand(AddFilesCommandExecuted);
            AddFromFolderCommand = new ViewModelCommand(AddFromFolderCommandExecuted);
            SaveToFolderCommand = new ViewModelCommand(SaveToFolderCommandExecuted);
            PackToFolderCommand = new ViewModelCommand(PackToFolderCommandExecuted);
            ShowInfoCommand = new ViewModelCommand(() => new InfoWindow().ShowDialog());
            SendPerMailCommand = new ViewModelCommand(SendPerMailCommandExecuted);
            UnifyImageNamesCommand = new ViewModelCommand(UnifyImageNamesCommandExecuted);

            PropertyChanged += OnViewModelPropertyChanged;
            _images.CollectionChanged += ImagesOnCollectionChanged;
            Scale = 100.0;
            Quality = 90;

            Statusbar = new StatusbarViewModel();
            _estimate = new EstimateThread(this);

            UpdateCommandStates();
        }

        private void UnifyImageNamesCommandExecuted()
        {
            var viewModel = new EnterNameDialogModel(e => ImageModel.UnifyImageNames(this, e.Value));
            ViewService.ShowDialog(viewModel);
        }

        private void UpdateCommandStates()
        {
            var anyImagesSelected = _images.Any(i => i.IsSelected);
            AddFilesCommand.Executable = true;
            AddFromFolderCommand.Executable = true;
            ShowInfoCommand.Executable = true;
            UnifyImageNamesCommand.Executable = anyImagesSelected;
            SendPerMailCommand.Executable = anyImagesSelected;
            SaveToFolderCommand.Executable = anyImagesSelected;
            PackToFolderCommand.Executable = anyImagesSelected;
        }

        private void SendPerMailCommandExecuted()
        {
            var eMailSendViewModel = new EMailSendViewModel(this);
            var window = new EMailSendWindow
                                         {
                                             DataContext = eMailSendViewModel,
                                             Owner = ViewService.MainWindow,
                                         };
            window.ShowDialog();
        }

        public StatusbarViewModel Statusbar
        {
            get { return _statusbar; }
            set { SetBackingField("Statusbar", ref _statusbar, value); }
        }

        public bool ImageDataChangedForCalculation
        {
            get { return _imageDataChangedForCalculation; }
            set
            {
                _imageDataChangedForCalculation = value;
                _timer.IsEnabled = _imageDataChangedForCalculation;
            }
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (ViewService.AsyncJobRunning) return;
            if (_images.Count(i => i.IsSelected) == 0) return;

            if (ImageDataChangedForCalculation)
            {
                _estimate.Start();
                ImageDataChangedForCalculation = false;
            }
        }

        private void ImagesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("SelectedImageCount");
            UpdateDesiredSize();
            UpdateCommandStates();
            HasImages = _images.Count > 0;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Scale")
            {
                OnPropertyChanged("DesiredWidth");
                OnPropertyChanged("DesiredHeight");
                ImageDataChangedForCalculation = true;
            }
            else if (e.PropertyName == "Quality")
            {
                ImageDataChangedForCalculation = true;
            }
        }

        private void UpdateDesiredSize()
        {
            _maxWidth = 0;
            _maxHeight = 0;
            foreach (var imageViewModel in _images)
            {
                if (imageViewModel.Width <= _maxWidth) continue;
                _maxWidth = imageViewModel.Width;
                _maxHeight = imageViewModel.Height;
            }
            OnPropertyChanged("DesiredWidth");
            OnPropertyChanged("DesiredHeight");
        }

        private void PackToFolderCommandExecuted()
        {
            string path;
            if (ViewService.ChooseFolderDialog(out path))
            {
                ViewService.ExecuteAsyncJobWithDialog(this, new ProgressDialogViewModel(), new PackToDirectoryJob(path));
            }
        }

        private void SaveToFolderCommandExecuted()
        {
            string path;
            if (ViewService.ChooseFolderDialog(out path))
                ViewService.ExecuteAsyncJobWithDialog(this, new ProgressDialogViewModel(), new CopyToFolderJob(path));
        }

        private void AddFromFolderCommandExecuted()
        {
            string path;
            if (ViewService.ChooseFolderDialog(out path))
                ViewService.ExecuteAsyncJobWithDialog(this, new ProgressDialogViewModel(), new LoadFromFolderJob(path));
            SelectFirstImage();
        }
        
        private void AddFilesCommandExecuted()
        {
            string[] files;
            if(ViewService.ChooseFilesDialog(out files))
            {
                if (files.Length <= 10)
                    foreach (var file in files) AddImage(ImageModel.CreateFromFile(file));
                else
                    ViewService.ExecuteAsyncJobWithDialog(this, new ProgressDialogViewModel(), new LoadFromFilesJob(files));
            }

            SelectFirstImage();
        }

        private void SelectFirstImage()
        {
            if (_images.Count > 0 && SelectedImage == null)
                SelectedImage = Images.First();
        }

        public void AddImage(ImageViewModel imageViewModel)
        {
            imageViewModel.Parent = this;
            imageViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != "IsSelected") return;
                ImageDataChangedForCalculation = true;
                OnPropertyChanged("SelectedImageCount");
                UpdateCommandStates();
            };
            _images.Add(imageViewModel);
            ImageDataChangedForCalculation = true;
        }

        public long Quality
        {
            get { return _quality; }
            set { SetBackingField("Quality", ref _quality, value); }
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

        public double CompressedSize
        {
            get { return _compressedSize; }
            set { SetBackingField("CompressedSize", ref _compressedSize, value); }
        }

        public bool HasImages
        {
            get { return _hasImages; }
            set { SetBackingField("HasImages", ref _hasImages, value); }
        }

        public int DesiredWidth { get { return (int) (_maxWidth * Scale * 0.01); } }
        public int DesiredHeight { get { return (int)(_maxHeight * Scale * 0.01); } }
        public int SelectedImageCount { get { return _images.Count(i => i.IsSelected); } }
    }
}
