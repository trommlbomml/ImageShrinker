
using System.Windows;

namespace ImageShrinker2.Windows
{
    public partial class InfoWindow
    {
        public InfoWindow()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
