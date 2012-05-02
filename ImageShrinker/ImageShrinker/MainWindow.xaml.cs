using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using ImageShrinker.Helper;
using ImageShrinker.ViewModels;
using ImageShrinker.BackgroundWork;

namespace ImageShrinker
{
    public partial class MainWindow
    {
        private readonly ImageShrinkerViewModel _imageShrinkerViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _imageShrinkerViewModel = new ImageShrinkerViewModel();
            DataContext = _imageShrinkerViewModel;
        }

        private void Save(bool packed)
        {
            try
            {
                string dir;
                if (GetDirectory(out dir))
                {
                    BackgroundJob job = new BackgroundJob { ImageShrinkerViewModel = _imageShrinkerViewModel };
                    if (packed)
                    {
                        job.PackToDirectory(this, dir);
                    }
                    else
                    {
                        job.SaveToDirectory(this, dir);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private bool GetDirectory(out string dir)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;

            if (dlg.ShowDialog(new OldWindow(mainWindowPtr)) == System.Windows.Forms.DialogResult.OK)
            {
                dir = dlg.SelectedPath;
                return true;
            }
            dir = String.Empty;
            return false;
        }

        void dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = (Microsoft.Win32.OpenFileDialog)sender;
            bool wasEmpty = _imageShrinkerViewModel.ImageThumbs.Count == 0;

            BackgroundJob job = new BackgroundJob { ImageShrinkerViewModel = _imageShrinkerViewModel };
            job.LoadImages(this, dlg.FileNames);

            if (wasEmpty && _imageShrinkerViewModel.ImageThumbs.Count > 0)
                _listViewThumbs.SelectedIndex = 0;
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListView lv = (System.Windows.Controls.ListView)e.Source;
            if (lv.SelectedIndex != -1)
            {
                _imageShrinkerViewModel.Select(lv.SelectedIndex);
            }
        }
        private void SelectAllImages_Click(object sender, RoutedEventArgs e)
        {
            _imageShrinkerViewModel.SelectAllImages(true);
        }
        private void DeSelectAllImages_Click(object sender, RoutedEventArgs e)
        {
            _imageShrinkerViewModel.SelectAllImages(false);
        }

        private void MenuItem_InfoClick(object sender, RoutedEventArgs e)
        {
            InfoWindow infoWindow = new InfoWindow { Owner = this };
            infoWindow.ShowDialog();
        }
        
        private void MenuItem_AddImagesClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Bilder auswählen";
            dlg.Multiselect = true;
            dlg.Filter = "Bildateien(*.jpg,*.jpeg)|*.jpg;*.jpeg";
            dlg.FileOk += dlg_FileOk;
            dlg.ShowDialog(this);
        }

        private void MenuItem_AddFromFolderClick(object sender, RoutedEventArgs e)
        {
            string dir;
            if (GetDirectory(out dir))
            {
                bool wasEmpty = _imageShrinkerViewModel.ImageThumbs.Count == 0;

                BackgroundJob job = new BackgroundJob { ImageShrinkerViewModel = _imageShrinkerViewModel };
                job.LoadImages(this,dir);

                if (wasEmpty && _imageShrinkerViewModel.ImageThumbs.Count > 0)
                    _listViewThumbs.SelectedIndex = 0;
            }
        }

        private void MenuItem_SaveToFolderClick(object sender, RoutedEventArgs e)
        {
            Save(false);
        }

        private void MenuItem_PackToClick(object sender, RoutedEventArgs e)
        {
            Save(true);
        }

        private void MenuItem_SendPerMailClick(object sender, RoutedEventArgs e)
        {
            EMailSendWindow mailSendWindow = new EMailSendWindow(_imageShrinkerViewModel)
                {
                    Owner = this,
                };
            mailSendWindow.ShowDialog();
        }

        private void Button_EstimateClick(object sender, RoutedEventArgs e)
        {
            BackgroundJob job = new BackgroundJob
                {
                    ImageShrinkerViewModel = _imageShrinkerViewModel,
                };
            job.CalculateEstimatedCompressedSize(this);
        }

        private void Button_RotateCCWClick(object sender, RoutedEventArgs e)
        {
            _imageShrinkerViewModel.PreViewRotation -= 90;
        }

        private void Button_RotateCWClick(object sender, RoutedEventArgs e)
        {
            _imageShrinkerViewModel.PreViewRotation += 90;
        }
    }
}
