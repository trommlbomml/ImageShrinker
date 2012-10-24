
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ImageShrinker2.Framework;
using ImageShrinker2.Model;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Jobs
{
    class CopyToFolderJob : AsyncJobBase
    {
        protected string CopyToDirectory;
        private int _progressCounter;

        public CopyToFolderJob(string directory)
        {
            CopyToDirectory = directory;
        }

        public override void Prepare(ImageShrinkerViewModel imageShrinkerViewModel, IBackgroundWorkerUi ui)
        {
            base.Prepare(imageShrinkerViewModel, ui);
            Ui.MessageText = "Deploy Images to Folder...";
            Ui.IsIndeterminate = false;
            Ui.ProgressMinimum = 0;
            Ui.ProgressMaximum = ImageShrinkerViewModel.SelectedImageCount - 1;
            _progressCounter = 0;
        }

        public override void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            string targetFolder = Path.Combine(CopyToDirectory, ImageShrinkerViewModel.ArchiveName);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            List<ImageViewModel> selectedImages = ImageShrinkerViewModel.Images.Where(i => i.IsSelected).ToList();
            foreach (ImageViewModel imageViewModel in selectedImages)
            {
                IncreasingProgress(string.Format("Speichere Bild {0} von {1} ...", ++_progressCounter, selectedImages.Count));
                ImageModel.SaveScaled(imageViewModel, targetFolder);
            }
        }
    }
}
