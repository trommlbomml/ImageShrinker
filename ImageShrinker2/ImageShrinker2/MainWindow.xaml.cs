
using ImageShrinker2.ViewModels;

namespace ImageShrinker2
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ImageShrinkerViewModel() {ArchiveName = "BilderArchiv"};
        }
    }
}
