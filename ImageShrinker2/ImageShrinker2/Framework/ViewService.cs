
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using ImageShrinker2.ViewModels;
using ImageShrinker2.Windows;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ImageShrinker2.Framework
{
    static class ViewService
    {
        private static readonly Dictionary<Type, Type> ViewModelToWindowMapping = new Dictionary<Type, Type>
        {
            {typeof(ProgressDialogViewModel), typeof(ProgressWindow)},
            {typeof(EnterNameDialogModel), typeof(EnterNameWindow)},
        };
 
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

        private static Window _eMailSendWindow;
        public static Window EMailSendWindow
        {
            get
            {
                if (_eMailSendWindow == null)
                    _eMailSendWindow = GetWindow(typeof(EMailSendWindow));
                return _eMailSendWindow;
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

        private static Type GetWindowForViewModel(ViewModel viewModel)
        {
            Type windowType;
            if (!ViewModelToWindowMapping.TryGetValue(viewModel.GetType(), out windowType))
            {
                throw new ArgumentException("No Window for ViewModel Type " + viewModel.GetType().FullName);
            }
            return windowType;
        }

        public static void ShowDialog(ViewModel dialog)
        {
            var active = WinApiInterop.GetActiveWindow();
            var activeWindow =
                Application.Current.Windows.OfType<Window>().SingleOrDefault(
                    w => new WindowInteropHelper(w).Handle == active);
            
            var windowType = GetWindowForViewModel(dialog);

            var window = (Window)Activator.CreateInstance(windowType);
            window.DataContext = dialog;
            window.Owner = activeWindow;

            window.ShowDialog();
        }

        public static void ExecuteAsyncJobWithDialog(ImageShrinkerViewModel context, IBackgroundWorkerUi uiResponder, IAsyncJob job)
        {
            ExecuteAsyncJob(context, uiResponder, job);
            ShowDialog((ViewModel)uiResponder);
        }

        public static void ExecuteAsyncJob(ImageShrinkerViewModel context, IBackgroundWorkerUi uiResponder, IAsyncJob job)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += job.BackgroundWorkerOnDoWork;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
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
