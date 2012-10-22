
using System;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ImageShrinker2.Framework
{
    static class ViewService
    {
        private static Window _mainWindow;
        private static Window MainWindow
        {
            get
            {
                if (_mainWindow == null)
                    _mainWindow = GetWindow(typeof(MainWindow));
                return _mainWindow;
            }
        }

        private static Window GetWindow(Type windowType)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == windowType)
                    return window;
            }

            throw new InvalidOperationException(string.Format("There is no Window of type '{0}'", windowType.FullName));
        }

        public static bool ChooseFilesDialog(out string[] file)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                file = openFileDialog.FileNames;
                return true;
            }

            file = null;
            return false;
        }

        public static bool ChooseFolderDialog(out string folder)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if(dialog.ShowDialog(MainWindow.GetIWin32Window()) == DialogResult.OK)
            {
                folder = dialog.SelectedPath;
                return true;
            }

            folder = null;
            return false;
        }
    }
}
