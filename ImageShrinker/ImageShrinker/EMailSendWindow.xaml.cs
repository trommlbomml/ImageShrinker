using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ImageShrinker.BackgroundWork;
using ImageShrinker.Helper;
using ImageShrinker.Properties;
using ImageShrinker.ViewModels;

namespace ImageShrinker
{
    public partial class EMailSendWindow : INotifyPropertyChanged
    {
        private readonly ImageShrinkerViewModel _imageShrinkerViewModel;
        private readonly AddressBookViewModel _addressBookViewModel;

        public EMailSendWindow(ImageShrinkerViewModel imageShrinkerViewModel)
        {
            InitializeComponent();
            DataContext = this;
            Loaded += EMailSendWindow_Loaded;
            _imageShrinkerViewModel = imageShrinkerViewModel;
            _addressBookViewModel = new AddressBookViewModel();
            Subject = string.Format("Bilder: {0}", imageShrinkerViewModel.ArchiveName);
        }

        void EMailSendWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedProviderIndex = Settings.Default.ProviderIndex;
            EMailAdress = Settings.Default.EMailAdress;
            UserName = Settings.Default.UserName;
            passwordBox.Password = Encryption.DecryptString(Settings.Default.UserPassword, "sdfjl4t93qzg89bmqrz9");
            CredentialsChanged = false;
            Subject = "Kein Betreff";
        }

        private bool CredentialsComplete()
        {
            if (SelectedProviderIndex == 0)
            {
                MessageBox.Show(
                    "Bitte wählen Sie einen Provider.",
                    "Fehler bei den Benutzerdaten",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return false;
            }
            
            if (String.IsNullOrEmpty(UserName))
            {
                MessageBox.Show(
                    "Bitte geben Sie Ihren Benutzernamen an.",
                    "Fehler bei den Benutzerdaten",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return false;
            }
            if (String.IsNullOrEmpty(EMailAdress))
            {
                MessageBox.Show(
                    "Bitte geben Sie Ihre E-Mail Adresse an.",
                    "Fehler bei den Benutzerdaten",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }

        private bool SendDataComplete()
        {
            if (!CredentialsComplete())
                return false;

            List<string> adresses = GetAdresses();
            if (adresses.Count == 0)
            {
                MessageBox.Show(
                    "Es wurde kein Adressat festgelegt.",
                    "Fehler bei den Benutzerdaten",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return false;
            }

            foreach (string adress in adresses)
            {
                if (!IsValidAdress(adress))
                {
                    MessageBox.Show(
                        String.Format("Die E-Mail Adresse '{0}' ist nicht gültig.", adress),
                        "Fehler bei den Benutzerdaten",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return false;
                }
            }

            return true;
        }

        private bool IsValidAdress(string adress)
        {
            return Regex.IsMatch(adress.ToUpperInvariant(), @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b");
        }

        private List<string> GetAdresses()
        {
            List<string> adresses = new List<string>();

            if (!String.IsNullOrEmpty(SendToAdresses))
            {
                foreach (string adress in SendToAdresses.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    string addressTrimmed = adress.Trim();
                    if (!string.IsNullOrEmpty(addressTrimmed))
                    {
                        adresses.Add(addressTrimmed);
                    }
                }
            }

            return adresses;
        }

        private void Button_SaveSettingsClick(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (CredentialsComplete())
            {
                Settings.Default.ProviderIndex = SelectedProviderIndex;
                Settings.Default.UserName = UserName;
                Settings.Default.EMailAdress = EMailAdress;
                Settings.Default.UserPassword = Encryption.EncryptString(passwordBox.Password, "sdfjl4t93qzg89bmqrz9");
                Settings.Default.Save();
            }
            CredentialsChanged = false;
            Cursor = Cursors.Arrow;
        }

        private void RequestForNewAddressesToBook()
        {
            List<string> notInAddressbook = new List<string>();
            foreach (string address in GetAdresses())
            {
                if (!_addressBookViewModel.AddressItems.Any(a => a.EMailAdress == address))
                {
                    notInAddressbook.Add(address);
                }
            }

            if (notInAddressbook.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Folgende Adressaten sind nicht im Adressbuch:\n\n");
                notInAddressbook.ForEach(s => stringBuilder.AppendFormat("{0}\n",s));
                stringBuilder.Append("\nMöchten Sie diese Adressen ins Buch hinzufügen?");

                if (MessageBox.Show(stringBuilder.ToString(), "Neue Adressen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    notInAddressbook.ForEach(s => _addressBookViewModel.AddressItems.Add(new AddressItem { IsSelected = true, EMailAdress = s }));
                    _addressBookViewModel.Save();
                }
            }
        }

        private void LoginPOP3()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(GetProviderPop3FromIndex(SelectedProviderIndex),110);

            NetworkStream netStream = tcpClient.GetStream();
            StreamReader strReader = new StreamReader(tcpClient.GetStream());

            if (tcpClient.Connected)
            {
                byte[] writeBuffer = new byte[1024];
                ASCIIEncoding enc = new ASCIIEncoding();

                writeBuffer = enc.GetBytes(string.Format("USER {0}\r\n", UserName));
                netStream.Write(writeBuffer, 0, writeBuffer.Length);
                string a = strReader.ReadLine();

                writeBuffer = enc.GetBytes(string.Format("PASS {0}\r\n", passwordBox.Password));
                netStream.Write(writeBuffer, 0, writeBuffer.Length);
                a = strReader.ReadLine();

                writeBuffer = enc.GetBytes("LIST\r\n");
                netStream.Write(writeBuffer, 0, writeBuffer.Length);

                while (true)
                {
                    string listMessage = strReader.ReadLine();
                    if (listMessage == ".")
                    {
                        break;
                    }
                }

                writeBuffer = enc.GetBytes("QUIT\r\n");
                netStream.Write(writeBuffer, 0, writeBuffer.Length);
                a = strReader.ReadLine();
            }
        }

        private static string GetProviderSMTPFromIndex(int index)
        {
            switch (index)
            {
                case 1: return Settings.Default.FreeNetSMTP;
                case 2: return Settings.Default.GmxSMTP;
                case 3: return Settings.Default.GoogleMailSMTP;
                case 4: return Settings.Default.HotmailSMTP;
                case 5: return Settings.Default.TOnlineSMTP;
                case 6: return Settings.Default.WebDeSMTP;
                default: return string.Empty;
            }
        }

        private static string GetProviderPop3FromIndex(int index)
        {
            switch (index)
            {
                case 1: return Settings.Default.FreeNetPOP3;
                case 2: return Settings.Default.GmxPOP3;
                case 3: return Settings.Default.GoogleMailPOP3;
                case 4: return Settings.Default.HotmailPOP3;
                case 5: return Settings.Default.TOnlinePOP3;
                case 6: return Settings.Default.WebDePOP3;
                default: return string.Empty;
            }
        }

        private string GetPlainText()
        {
            return mailText.Text;
        }

        private void Button_SendClick(object sender, RoutedEventArgs e)
        {
            if (SendDataComplete())
            {
                RequestForNewAddressesToBook();

                //LoginPOP3();

                MailMessage message = new MailMessage();
                message.From = new MailAddress(EMailAdress);
                GetAdresses().ForEach(s => message.To.Add(new MailAddress(s)));

                message.Body = GetPlainText();
                message.Subject = Subject;

                SmtpClient smtpClient = new SmtpClient() 
                {
                    UseDefaultCredentials = false,
                    Host = GetProviderSMTPFromIndex(SelectedProviderIndex),
                    Credentials = new NetworkCredential(UserName, passwordBox.Password),
                };

                BackgroundJob job = new BackgroundJob() { ImageShrinkerViewModel = _imageShrinkerViewModel };
                job.SendMail(this, smtpClient, message);
            }
        }

        private int _selectedProviderIndex;
        public int SelectedProviderIndex
        {
            get { return _selectedProviderIndex; }
            set 
            {
                if (_selectedProviderIndex != value)
                {
                    _selectedProviderIndex = value;
                    NotifyPropertyChanged("SelectedProviderIndex");
                    CredentialsChanged = true;
                }
            }
        }

        private string _eMailAdress;
        public string EMailAdress
        {
            get { return _eMailAdress; }
            set
            {
                if (_eMailAdress != value)
                {
                    _eMailAdress = value;
                    NotifyPropertyChanged("EMailAdress");
                    CredentialsChanged = true;
                }
            }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    NotifyPropertyChanged("UserName");
                    CredentialsChanged = true;
                }
            }
        }

        private string _subject;
        public string Subject
        {
            get { return _subject; }
            set
            {
                if (_subject != value)
                {
                    _subject = value;
                    NotifyPropertyChanged("Subject");
                }
            }
        }

        private string _sendToAdresses;
        public string SendToAdresses
        {
            get { return _sendToAdresses; }
            set
            {
                if (_sendToAdresses != value)
                {
                    _sendToAdresses = value;
                    NotifyPropertyChanged("SendToAdresses");
                }
            }
        }

        private bool _credentialsChanged;
        public bool CredentialsChanged
        {
            get { return _credentialsChanged; }
            set 
            {
                _credentialsChanged = value;
                NotifyPropertyChanged("CredentialsChanged");
            }
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CredentialsChanged = true;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        private void Button_FromAdressBookClick(object sender, RoutedEventArgs e)
        {
            AddressbookWindow addressBookWindow = new AddressbookWindow(_addressBookViewModel)
                {
                    Owner = this,
                };

            if (addressBookWindow.ShowDialog() == true)
            {
                List<string> addresses = GetAdresses();
                foreach (AddressItem item in _addressBookViewModel.AddressItems.Where(a => a.IsSelected))
                {
                    if (!addresses.Contains(item.EMailAdress))
                        addresses.Add(item.EMailAdress);
                }

                StringBuilder stringBuilder = new StringBuilder();
                foreach (string address in addresses)
                {
                    stringBuilder.Append(address);
                    stringBuilder.Append("; ");
                }
                SendToAdresses = stringBuilder.ToString();
            }
        }

        private void Button_AbortClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
