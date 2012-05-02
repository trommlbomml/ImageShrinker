using System.Windows;
using ImageShrinker.ViewModels;

namespace ImageShrinker
{
    public partial class AddressItemWindow
    {
        public AddressItem AddressItem { get; set; }

        public AddressItemWindow()
        {
            InitializeComponent();
            Loaded += AddressItemWindow_Loaded;
        }

        void AddressItemWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (AddressItem == null)
                AddressItem = new AddressItem();
            DataContext = AddressItem;
        }

        private void Button_OKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
