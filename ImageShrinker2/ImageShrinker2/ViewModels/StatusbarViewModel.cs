using System;
using System.ComponentModel;
using System.Windows.Threading;
using ImageShrinker2.Framework;

namespace ImageShrinker2.ViewModels
{
    class StatusbarViewModel : ViewModel, IBackgroundWorkerUi
    {
        private string _messageText;
        private double _progressMinimum;
        private double _progressMaximum;
        private double _progressValue;
        private bool _isIndeterminate;
        private bool _visible;

        public string MessageText
        {
            get { return _messageText; }
            set { SetBackingField("MessageText", ref _messageText, value); }
        }

        public double ProgressMinimum
        {
            get { return _progressMinimum; }
            set { SetBackingField("ProgressMinimum", ref _progressMinimum, value); }
        }

        public double ProgressMaximum
        {
            get { return _progressMaximum; }
            set { SetBackingField("ProgressMaximum", ref _progressMaximum, value); }
        }

        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetBackingField("ProgressValue", ref _progressValue, value); }
        }

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set { SetBackingField("IsIndeterminate", ref _isIndeterminate, value); }
        }

        public void OnWorkerCompleted()
        {
            Visible = false;
        }

        public void AfterAsyncStart()
        {
            Visible = true;
        }

        public bool Visible
        {
            get { return _visible; }
            private set { SetBackingField("Visible", ref _visible, value); }
        }

        public BackgroundWorker Worker { get; set; }
    }
}
