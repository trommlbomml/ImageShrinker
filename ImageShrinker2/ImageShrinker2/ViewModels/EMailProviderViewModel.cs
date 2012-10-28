
using ImageShrinker2.Framework;

namespace ImageShrinker2.ViewModels
{
    class EMailProviderViewModel : ViewModel
    {
        private string _name;
        private string _smpt;

        public string Name
        {
            get { return _name; }
            set { SetBackingField("Name", ref _name, value); }
        }

        public string Smpt
        {
            get { return _smpt; }
            set { SetBackingField("Smpt", ref _smpt, value); }
        }
    }
}
