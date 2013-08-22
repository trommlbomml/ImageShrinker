
using System.Windows;

namespace ImageShrinker2.Windows
{
    public partial class EnterNameWindow
    {
        public EnterNameWindow()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
