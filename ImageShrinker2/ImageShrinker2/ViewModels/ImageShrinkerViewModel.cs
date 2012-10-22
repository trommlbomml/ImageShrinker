
using System;
using System.Collections.ObjectModel;
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
                ImageModel.SaveScaled(imageViewModel, path, 320, 240, 100);
            }
        }

        private void AddFromFolderCommandExecuted()
        {
            string path;
            if (ViewService.ChooseFolderDialog(out path))
            {
                AddFromFolderInternal(path);
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
    }
}
