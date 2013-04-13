using System;
using System.Runtime.InteropServices;

namespace ImageShrinker2.Framework
{
    static class WinApiInterop
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();
    }
}
