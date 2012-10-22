using System;
using System.Windows.Forms;

namespace ImageShrinker2.Framework
{
    public static class WindowExtension
    {
        public static IWin32Window GetIWin32Window(this System.Windows.Media.Visual visual)
        {
            var source = System.Windows.PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : IWin32Window
        {
            private readonly IntPtr _handle;
            public OldWindow(IntPtr handle)
            {
                _handle = handle;
            }

            IntPtr IWin32Window.Handle { get { return _handle; } }
        }
    }
}
