
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContext = new ImageShrinkerViewModel {ArchiveName = "BilderArchiv"};
            Closing += MainWindowClosing;
            //MessageText = "ImageShrinker: Ruht";
        }

        void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (Worker != null && Worker.IsBusy)
                Worker.CancelAsync();
        }

        public BackgroundWorker Worker { get; set; }

        //public string MessageText
        //{
        //    get { return _asyncBackgroundText.Text; }
        //    set { _asyncBackgroundText.Text = value; }
        //}

        //public double ProgressMinimum
        //{
        //    get { return _progressBar.Minimum; }
        //    set { _progressBar.Minimum = value; }
        //}

        //public double ProgressMaximum
        //{
        //    get { return _progressBar.Maximum; }
        //    set { _progressBar.Maximum = value; }
        //}

        //public double ProgressValue
        //{
        //    get { return _progressBar.Value; }
        //    set { _progressBar.Value = value; }
        //}

        //public bool IsIndeterminate
        //{
        //    get { return _progressBar.IsIndeterminate; }
        //    set { _progressBar.IsIndeterminate = value; }
        //}

        //public void OnWorkerCompleted()
        //{
        //    MessageText = "ImageShrinker: Ruht";
        //    _progressBar.Visibility = Visibility.Hidden;
        //}

        //public void AfterAsyncStart()
        //{
        //    _progressBar.Visibility = Visibility.Visible;
        //}

        //public Dispatcher UpdateDispatcher { get { return Dispatcher; } }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var closeButton = (Button)Template.FindName("PART_CLOSE", this);
            closeButton.Click += (s,e) => Close();

            var minimizeButton = (Button)Template.FindName("PART_MINIMIZE", this);
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;

            var maxButton = (Button)Template.FindName("PART_MAXIMIZE_RESTORE", this);
            maxButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; ;

            var border = (Border)Template.FindName("PART_TITLEBAR", this);
            border.MouseMove += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
        }
    }
}
