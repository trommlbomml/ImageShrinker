using System.ComponentModel;
using System.Windows.Threading;

namespace ImageShrinker
{
    public partial class ProgressWindow : IBackgroundWorkerUI
    {
        public ProgressWindow()
        {
            InitializeComponent();
            Aborting = false;
            Closing += ProgressWindow_Closing;
        }

        void ProgressWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Aborting)
                Worker.CancelAsync();
            e.Cancel = Aborting;
        }

        public BackgroundWorker Worker { get; set; }
        public bool Aborting { get; set; }

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
