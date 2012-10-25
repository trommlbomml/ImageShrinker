
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using ImageShrinker2.ViewModels;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ImageShrinker2.Framework
{
    static class ViewService
    {
        private static Window _mainWindow;
        public static Window MainWindow
        {
            get
            {
                if (_mainWindow == null)
                    _mainWindow = GetWindow(typeof(MainWindow));
                return _mainWindow;
            }
        }

        public static bool AsyncJobRunning { get; private set; }

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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Bilddateien (*.bmp;*.jpg;*.jpeg;*.png)|*.bmp;*.jpg;*.jpeg;*.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Title = "Bitte Bilder wählen",
            };
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
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Bitte wählen Sie einen Zielordner aus:",
                ShowNewFolderButton = true,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if(dialog.ShowDialog(MainWindow.GetIWin32Window()) == DialogResult.OK)
            {
                folder = dialog.SelectedPath;
                return true;
            }

            folder = null;
            return false;
        }

        public static void ExecuteAsyncJob(ImageShrinkerViewModel context, IBackgroundWorkerUi uiResponder, IAsyncJob job)
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += job.BackgroundWorkerOnDoWork;
            backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                uiResponder.Aborting = false; 
                uiResponder.OnWorkerCompleted();
                AsyncJobRunning = false;
            };
            uiResponder.Worker = backgroundWorker;
            job.Prepare(context, uiResponder);
            AsyncJobRunning = true;
            backgroundWorker.RunWorkerAsync();
            uiResponder.AfterAsyncStart();
        }
    }
}
