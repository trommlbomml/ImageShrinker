
using System.ComponentModel;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Framework
{
    interface IAsyncJob
    {
        void Prepare(ImageShrinkerViewModel imageShrinkerViewModel, IBackgroundWorkerUi ui);
        void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs);
    }
}
