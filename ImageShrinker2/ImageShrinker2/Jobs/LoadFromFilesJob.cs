
using System;
using System.ComponentModel;
using System.IO;
using ImageShrinker2.Framework;
using ImageShrinker2.Model;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Jobs
{
    class LoadFromFilesJob : AsyncJobBase
    {
        private readonly string[] _files;
        private int _count;

        public LoadFromFilesJob(string[] files)
        {
            _files = files;
        }

        public override void Prepare(ImageShrinkerViewModel imageShrinkerViewModel, IBackgroundWorkerUi ui)
        {
            base.Prepare(imageShrinkerViewModel, ui);
            Ui.IsIndeterminate = false;
            Ui.ProgressMinimum = 0;
            Ui.ProgressMaximum = _files.Length - 1;
        }

        public override void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            foreach (string file in _files)
            {
                if (new FileInfo(file).Attributes == FileAttributes.Directory) continue;

                string extension = Path.GetExtension(file);
                if (string.IsNullOrEmpty(extension)) continue;

                extension = extension.ToLower();
                if (extension == ".jpg" || extension == ".jpeg")
                {
                    ImageViewModel imageViewModel = ImageModel.CreateFromFile(file);
                    string message = String.Format("Lade Bild {0} von {1}...", ++_count, _files.Length);
                    InvokeIncreasingProgress(message, () => ImageShrinkerViewModel.AddImage(imageViewModel));
                }
            }
        }
    }
}
