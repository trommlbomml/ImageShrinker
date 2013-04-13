using System;
using System.ComponentModel;
using ImageShrinker2.Framework;

namespace ImageShrinker2.ViewModels
{
    class ProgressDialogViewModel : ViewModel, IBackgroundWorkerUi, ICloseInteraction
    {
        public ViewModelCommand CancelCommand { get; private set; }

        private string _messageText;
        private double _progressMinimum;
        private double _progressMaximum;
        private double _progressValue;
        private bool _isIndeterminate;

        public ProgressDialogViewModel()
        {
            CancelCommand = new ViewModelCommand(OnCancelExecute);
        }

        private void OnCancelExecute()
        {
            Worker.CancelAsync();
        }

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
            if (RequestClose != null) RequestClose(this, EventArgs.Empty);
        }

        public void AfterAsyncStart()
        {
            
        }

        public BackgroundWorker Worker { get; set; }

        public bool OnClose()
        {
            Worker.CancelAsync();
            return true;
        }

        public event EventHandler RequestClose;
    }
}
