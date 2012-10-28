
using System.ComponentModel;
using System.Windows.Threading;
using ImageShrinker2.Framework;

namespace ImageShrinker2.Windows
{
    public partial class ProgressWindow : IBackgroundWorkerUi
    {
        public ProgressWindow()
        {
            InitializeComponent();
            Closing += ProgressWindowClosing;
        }

        void ProgressWindowClosing(object sender, CancelEventArgs e)
        {
            if (Worker != null && Worker.IsBusy)
                Worker.CancelAsync();
        }

        public BackgroundWorker Worker { get; set; }

        public string MessageText
        {
            get { return _informationText.Text; }
            set { _informationText.Text = value; }
        }

        public double ProgressMinimum
        {
            get { return _progressBar.Minimum; }
            set { _progressBar.Minimum = value; }
        }

        public double ProgressMaximum
        {
            get { return _progressBar.Maximum; }
            set { _progressBar.Maximum = value; }
        }

        public double ProgressValue
        {
            get { return _progressBar.Value; }
            set { _progressBar.Value = value; }
        }

        public bool IsIndeterminate
        {
            get { return _progressBar.IsIndeterminate; }
            set { _progressBar.IsIndeterminate = value; }
        }


        public void OnWorkerCompleted()
        {
            Close();
        }

        public void AfterAsyncStart()
        {
            ShowDialog();
        }

        public Dispatcher UpdateDispatcher
        {
            get { return Dispatcher; }
        }
    }
}
