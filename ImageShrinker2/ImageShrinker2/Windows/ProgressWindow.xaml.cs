
using System;
using System.ComponentModel;
using System.Windows;
using ImageShrinker2.Framework;

namespace ImageShrinker2.Windows
{
    public partial class ProgressWindow
    {
        private ICloseInteraction _closeInteraction;
        private readonly EventHandler _requestCloseHandler;

        public ProgressWindow()
        {
            InitializeComponent();
            _requestCloseHandler = (s, e) => Close();
            DataContextChanged += OnDataContextChanged;
            Closing += ProgressWindowClosing;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_closeInteraction != null) _closeInteraction.RequestClose -= _requestCloseHandler;
            if (e.NewValue is ICloseInteraction)
            {
                _closeInteraction = (ICloseInteraction) e.NewValue;
                _closeInteraction.RequestClose += _requestCloseHandler;
            }
        }

        void ProgressWindowClosing(object sender, CancelEventArgs e)
        {
            if (_closeInteraction == null) return;
            e.Cancel = !_closeInteraction.OnClose();
        }
    }
}
