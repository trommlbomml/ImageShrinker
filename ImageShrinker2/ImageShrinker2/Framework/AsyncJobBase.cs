
using System;
using System.ComponentModel;
using System.Windows.Threading;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Framework
{
    abstract class AsyncJobBase : IAsyncJob
    {
        protected ImageShrinkerViewModel ImageShrinkerViewModel { get; private set; }
        protected IBackgroundWorkerUi Ui { get; private set; }
        protected bool CancellationPending { get { return Ui.Worker.CancellationPending; } }

        protected void InvokeIncreasingProgress(Action action)
        {
            Ui.UpdateDispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                                                                      {
                                                                          Ui.ProgressValue += 1;
                                                                          action();
                                                                      });
        }

        protected void InvokeIncreasingProgress(string text, Action action)
        {
            Ui.UpdateDispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                Ui.ProgressValue += 1;
                Ui.MessageText = text;
                action();
            });
        }

        public virtual void Prepare(ImageShrinkerViewModel imageShrinkerViewModel, IBackgroundWorkerUi ui)
        {
            ImageShrinkerViewModel = imageShrinkerViewModel;
            Ui = ui;
        }

        public abstract void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs);
    }
}
