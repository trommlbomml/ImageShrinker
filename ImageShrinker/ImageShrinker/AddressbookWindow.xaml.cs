
using System.Windows;
using ImageShrinker.ViewModels;

namespace ImageShrinker
{
    public partial class AddressbookWindow
    {
        private readonly AddressBookViewModel _addressBookViewModel;

        public AddressbookWindow(AddressBookViewModel addressBookViewModel)
        {
            InitializeComponent();
            _addressBookViewModel = addressBookViewModel;
            DataContext = addressBookViewModel;
        }

        private void Button_OKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_EditAddressClick(object sender, RoutedEventArgs e)
        {
            AddressItem addressItem = _addressBookViewModel.AddressItems[addressListView.SelectedIndex];

            AddressItem addressItemOld = new AddressItem
                {
                    Name = addressItem.Name,
                    EMailAdress = addressItem.EMailAdress,
                    IsSelected = addressItem.IsSelected,
                };

            AddressItemWindow addressItemWindow = new AddressItemWindow
                {
                    Owner = this,
                    AddressItem = addressItem,
                };

            if (addressItemWindow.ShowDialog() == true)
            {
                _addressBookViewModel.AddressItems[addressListView.SelectedIndex] = addressItemWindow.AddressItem;
            }
            else
            {
                _addressBookViewModel.AddressItems[addressListView.SelectedIndex] = addressItemOld;
            }
            _addressBookViewModel.Save();
        }

        private void Button_AddAddressClick(object sender, RoutedEventArgs e)
        {
            AddressItemWindow addressItemWindow = new AddressItemWindow
            {
                Owner = this,
            };

            if (addressItemWindow.ShowDialog() == true)
            {
                _addressBookViewModel.AddressItems.Add(addressItemWindow.AddressItem);
            }
            _addressBookViewModel.Save();
        }

        private void Button_DeleteAddressClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie den Kontakt wirklich löschen?", "Bestätigung", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _addressBookViewModel.AddressItems.RemoveAt(addressListView.SelectedIndex);
            }
            _addressBookViewModel.Save();
        }
    }
}
