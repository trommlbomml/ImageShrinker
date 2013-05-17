
using System;

namespace ImageShrinker2.ViewModels
{
    class EnterNameDialogModel : DialogViewModel
    {
        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetBackingField("Value", ref _value, value); }
        }
        
        public EnterNameDialogModel(Action<EnterNameDialogModel> onOk)
            : base("Namen eingeben", f => onOk((EnterNameDialogModel)f))
        {
        }
    }
}
