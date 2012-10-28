
using System.Windows;
using System.Windows.Controls;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Windows
{
    public partial class EMailSendWindow
    {
        public EMailSendWindow()
        {
            InitializeComponent();
        }

        private void PasswordPasswordChanged(object sender, RoutedEventArgs e)
        {
            EMailSendViewModel eMailSendViewModel = DataContext as EMailSendViewModel;
            if (eMailSendViewModel == null) return;

            PasswordBox passwordBox = (PasswordBox) sender;
            eMailSendViewModel.Password = passwordBox.SecurePassword;
        }
    }
}
