
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using ImageShrinker2.ViewModels;
using log4net;

namespace ImageShrinker2.Framework
{
    abstract class AsyncJobBase : IAsyncJob
    {
        protected ImageShrinkerViewModel ImageShrinkerViewModel { get; private set; }
        protected IBackgroundWorkerUi Ui { get; private set; }
        protected bool CancellationPending { get { return Ui.Worker.CancellationPending; } }

        protected void InvokeIncreasingProgress(Action action)
        {
            Ui.UpdateDispatcher.Invoke(DispatcherPriority.Normal, 
                                       (Action)delegate { Ui.ProgressValue += 1; action(); });
        }

        protected void InvokeIncreasingProgress(string text, Action action)
        {
            Ui.UpdateDispatcher.Invoke(DispatcherPriority.Normal,
                                       (Action)delegate { Ui.ProgressValue += 1; Ui.MessageText = text; action(); });
        }

        protected void IncreasingProgress(string text)
        {
            InvokeIncreasingProgress(text, () => { });
        }

        public virtual void Prepare(ImageShrinkerViewModel imageShrinkerViewModel, IBackgroundWorkerUi ui)
        {
            ImageShrinkerViewModel = imageShrinkerViewModel;
            Ui = ui;
        }

        protected void HandleError(Exception ex, string logBase, string caption, string messageBoxText)
        {
            LogManager.GetLogger(typeof(AsyncJobBase)).Error(logBase, ex);
            MessageBox.Show(caption, messageBoxText, MessageBoxButton.OK);
        }

        public abstract void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs);
    }
}
