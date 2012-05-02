using System.Windows;

namespace ImageShrinker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
