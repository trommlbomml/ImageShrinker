
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ImageShrinker2.Framework;
using ImageShrinker2.Model;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Jobs
{
    class CalculateCompressedSizeJob : AsyncJobBase
    {
        private int _progressCounter;
        
        public override void Prepare(ImageShrinkerViewModel imageShrinkerViewModel, IBackgroundWorkerUi ui)
        {
            base.Prepare(imageShrinkerViewModel, ui);
            Ui.MessageText = "Calculating Compressed Size";
            Ui.IsIndeterminate = false;
            Ui.ProgressMinimum = 0;
            Ui.ProgressValue = 0;
            Ui.ProgressMaximum = ImageShrinkerViewModel.SelectedImageCount - 1;
            _progressCounter = 0;
        }

        public override void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            ImageShrinkerViewModel.CompressedSize = 0;
            double size = 0;
            var lockObject = new object();
            var selectedImages = ImageShrinkerViewModel.Images.Where(i => i.IsSelected).ToList();
            var result = Parallel.ForEach(selectedImages,
                                (i,p) =>
                                {
                                    if (Ui.Worker.CancellationPending) p.Stop();

                                    var messageText = "Calculating Compressed Size" +
                                                         new string('.', (++_progressCounter) % 4);
                                    IncreasingProgress(messageText);

                                    var t = ImageModel.GetFileSizeScaledInMegaByte(i);
                                    lock (lockObject)
                                        size += t;
                                });
            Ui.MessageText = "Ready.";
            if (!result.IsCompleted) return;
            ImageShrinkerViewModel.CompressedSize = size;
            ImageShrinkerViewModel.ImageDataChangedForCalculation = false;
        }

    }
}
