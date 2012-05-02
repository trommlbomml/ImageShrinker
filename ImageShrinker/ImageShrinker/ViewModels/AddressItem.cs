
namespace ImageShrinker.ViewModels
{
    public class AddressItem : ViewModel
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set 
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private string _emailAdress;
        public string EMailAdress
        {
            get { return _emailAdress; }
            set
            {
                _emailAdress = value;
                NotifyPropertyChanged("EMailAdress");
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }
    }
}
