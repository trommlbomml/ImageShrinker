
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using ImageShrinker2.Framework;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2
{
    public partial class MainWindow : IBackgroundWorkerUi
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ImageShrinkerViewModel {ArchiveName = "BilderArchiv"};
            Closing += MainWindowClosing;
            MessageText = "ImageShrinker: Ruht";
            _progressBar.Visibility = Visibility.Collapsed;
        }

        void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (Worker != null && Worker.IsBusy)
                Worker.CancelAsync();
        }

        public BackgroundWorker Worker { get; set; }

        public string MessageText
        {
            get { return _asyncBackgroundText.Text; }
            set { _asyncBackgroundText.Text = value; }
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
            MessageText = "ImageShrinker: Ruht";
            _progressBar.Visibility = Visibility.Hidden;
        }

        public void AfterAsyncStart()
        {
            _progressBar.Visibility = Visibility.Visible;
        }

        public Dispatcher UpdateDispatcher { get { return Dispatcher; } }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
        }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            DataObject dataObject = e.Data as DataObject;
            if (dataObject == null) return;
            string t = dataObject.GetText();
        }
    }
}
