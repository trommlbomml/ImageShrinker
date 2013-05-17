
using System;
using ImageShrinker2.Framework;

namespace ImageShrinker2.ViewModels
{
    class DialogViewModel : ViewModel
    {
        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set { SetBackingField("Caption", ref _caption, value); }
        }

        public ViewModelCommand AcceptCommand { get; private set; }

        public DialogViewModel(string caption, Action<DialogViewModel> onAccept)
        {
            Caption = caption;
            AcceptCommand = new ViewModelCommand(() => onAccept(this));
        }
    }
}
