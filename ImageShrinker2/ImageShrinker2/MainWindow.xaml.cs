
using System.ComponentModel;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ImageShrinkerViewModel {ArchiveName = "BilderArchiv"};
            Closing += MainWindowClosing;
        }

        void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (Worker != null && Worker.IsBusy) Worker.CancelAsync();
        }

        public BackgroundWorker Worker { get; set; }
    }
}
