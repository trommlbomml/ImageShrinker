
using System.Collections.ObjectModel;
using System.Security;
using ImageShrinker2.Framework;

namespace ImageShrinker2.ViewModels
{
    class EMailSendViewModel : ViewModel
    {
        private string _message;
        private string _adressors;
        private string _copyAdressors;
        private string _eMailAdress;

        private readonly ObservableCollection<EMailProviderViewModel> _providers;
        private EMailProviderViewModel _selectedProvider;
        private SecureString _password;

        public EMailSendViewModel()
        {
            _providers = new ObservableCollection<EMailProviderViewModel>();
        }

        public ReadOnlyObservableCollection<EMailProviderViewModel> Providers
        {
            get {return new ReadOnlyObservableCollection<EMailProviderViewModel>(_providers);}
        }

        public EMailProviderViewModel SelectedProvider
        {
            get { return _selectedProvider; }
            set { SetBackingField("SelectedProvider", ref _selectedProvider, value); }
        }

        public string Message
        {
            get { return _message; }
            set { SetBackingField("Message", ref _message, value); }
        }

        public string Adressors
        {
            get { return _adressors; }
            set { SetBackingField("Adressors", ref _adressors, value); }
        }

        public string CopyAdressors
        {
            get { return _copyAdressors; }
            set { SetBackingField("CopyAdressors", ref _copyAdressors, value); }
        }

        public string EMailAdress
        {
            get { return _eMailAdress; }
            set { SetBackingField("EMailAdress", ref _eMailAdress, value); }
        }

        public SecureString Password
        {
            get { return _password; }
            set { SetBackingField("Password", ref _password, value); }
        }
    }
}
