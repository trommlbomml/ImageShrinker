
using System.Windows;
using System.ComponentModel;
using ImageShrinker.ViewModels;
using System.IO;
using Ionic.Zip;
using System;
using System.Net.Mail;
using System.Threading;
using System.Linq;
using ImageShrinker.Helper;

namespace ImageShrinker.BackgroundWork
{
    class BackgroundJob
    {
        private BackgroundWorker _worker;
        private IBackgroundWorkerUI _backgroundUI;

        private string TempPath 
        { 
            get 
            { 
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ImageShrinker");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            } 
        }

        private string _saveToPath;
        private string[] _loadToFileNames;
        private SmtpClient _smtpClient;
        private MailMessage _mailMessage;
        public ImageShrinkerViewModel ImageShrinkerViewModel { get; set; }

        private void DoWorkerJob(IBackgroundWorkerUI backgroundUI, DoWorkEventHandler handler)
        {
            _worker = new BackgroundWorker();
            _backgroundUI = backgroundUI;
            _backgroundUI.Worker = _worker;
            _worker.DoWork += handler;

            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerCompleted += delegate
            {
                backgroundUI.Aborting = false;
                backgroundUI.OnWorkerCompleted();
            };
            _worker.RunWorkerAsync();
            _backgroundUI.AfterAsyncStart();
        }

        public void CalculateEstimatedCompressedSize(Window owner)
        {
            ProgressWindow progressWindow = new ProgressWindow
            {
                IsIndeterminate = false,
                ProgressMinimum = 0,
                ProgressMaximum = ImageShrinkerViewModel.SelectedImageCount,
                Title = "Geschätze Archivgröße",
                Owner = owner,
            };

            DoWorkerJob(progressWindow, CalculateEstimatedCompressedSizeWorker);
        }

        private void CalculateEstimatedCompressedSizeWorker(object sender, DoWorkEventArgs args)
        {
            int toSave = ImageShrinkerViewModel.SelectedImageCount;
            int calculated = 0;
            double size = 0;

            foreach (ImageThumbViewModel thumb in ImageShrinkerViewModel.ImageThumbs.Where(t => t.IsSelected))
            {
                if (_worker.CancellationPending)
                {
                    break;
                }
                string progress = string.Format("Berechne Bildgröße {0} von {1}", ++calculated, toSave);
                _backgroundUI.UpdateDispatcher.BeginInvoke(new UpdateProgressDelegate(UpdateProgress), progress, calculated);

                size += thumb.GetFileSizeScaled((int)ImageShrinkerViewModel.DesiredSize.Width,ImageShrinkerViewModel.Quality);
            }

            if (!_worker.CancellationPending)
            {
                string text;
                MessageBoxImage messageBoxImage = MessageBoxImage.Information;
                if (size > 20)
                {
                    text = "Geschätzte Archivgröße: {0} MB.\n Ihr Archiv ist vermutlich für den E-Mail versand zu groß. 20MB ist das empfohlene Maximum.";
                    messageBoxImage = MessageBoxImage.Exclamation;
                }
                else
                {
                    text = "Geschätzte Archivgröße: {0} MB.\n Die Archivgröße ist in Ordnung.";
                }

                MessageBox.Show(string.Format(text,Math.Round(size,1)), "Archivgröße", MessageBoxButton.OK,messageBoxImage);
            }
        }

        public void SendMail(Window owner, SmtpClient smtpClient, MailMessage mailMessage)
        {
            _saveToPath = TempPath;
            _smtpClient = smtpClient;
            _mailMessage = mailMessage;

            ProgressWindow progressWindow = new ProgressWindow
            {
                IsIndeterminate = false,
                ProgressMinimum = 0,
                ProgressMaximum = ImageShrinkerViewModel.SelectedImageCount + 2 + 2,
                Title = "Mail versenden",
                Owner = owner,
            };

            DoWorkerJob(progressWindow, SendMailWorker);
        }

        private static bool _sendReady;

        private void SendMailWorker(object sender, DoWorkEventArgs args)
        {
            PackToWorker(null, new DoWorkEventArgs(null));

            string attachmentPath = Path.Combine(TempPath, ImageShrinkerViewModel.ArchiveName) + ".zip";

            try
            {
                _backgroundUI.UpdateDispatcher.BeginInvoke(new IncreasingUpdateProgressDelegate(IncreasingUpdateProgress), "E-Mail wird versendet...");
                _mailMessage.Attachments.Add(new Attachment(attachmentPath));

                _smtpClient.SendCompleted += _smtpClient_SendCompleted;
                _smtpClient.SendAsync(_mailMessage, null);

                while (!_sendReady)
                    Thread.Sleep(100);

                if (!string.IsNullOrEmpty(_errorMessage))
                    throw new Exception(_errorMessage);

                _smtpClient = null;
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                MessageBox.Show(String.Format("Die E-Mail konnte nicht versendet werden:\n{0}",ex.Message),"Information",MessageBoxButton.OK,MessageBoxImage.Exclamation);
                _backgroundUI.Aborting = true;
            }
            finally
            {
                _backgroundUI.UpdateDispatcher.BeginInvoke(new IncreasingUpdateProgressDelegate(IncreasingUpdateProgress), "Aufräumen...");

                if (_mailMessage != null)
                    _mailMessage.Dispose();

                if (File.Exists(attachmentPath))
                    File.Delete(attachmentPath);

                _backgroundUI.UpdateDispatcher.BeginInvoke(new UpdateProgressDelegate(UpdateProgress), String.Empty, 0);

                _errorMessage = string.Empty;
            }

            if (!_backgroundUI.Aborting)
            {
                MessageBox.Show("E-Mail wurde erfolgreich versendet.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static string _errorMessage = string.Empty;

        void _smtpClient_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                _errorMessage = "E-Mail versand wurde abgebrochen.";
            }
            else if (e.Error != null)
            {
                _errorMessage = e.Error.Message;
            }
            _sendReady = true;
        }

        public void LoadImages(Window owner, string directory)
        {
            ProgressWindow progressWindow = new ProgressWindow
            {
                Owner = owner,
                ProgressMinimum = 0,
                ProgressMaximum = 1,
                IsIndeterminate = true,
                Title = "Bilder aus Verzeichnis laden",
            };
            _saveToPath = directory;
            DoWorkerJob(progressWindow, LoadFromDirectoryWorker);
        }

        public void LoadImages(Window owner, string[] files)
        {
            ProgressWindow progressWindow = new ProgressWindow
            {
                Owner = owner,
                ProgressMinimum = 0,
                ProgressMaximum = files.Length,
                IsIndeterminate = false,
                Title = "Bilder Laden",
            };
            _loadToFileNames = files;
            DoWorkerJob(progressWindow, LoadFilesWorker);
        }

        public void PackToDirectory(Window owner, string path)
        {
            _saveToPath = path;
            ProgressWindow progressWindow = new ProgressWindow
                {
                    ProgressMaximum = ImageShrinkerViewModel.SelectedImageCount + 2,
                    Title = "Als Archiv packen",
                };
            DoWorkerJob(progressWindow, PackToWorker);
        }

        public void SaveToDirectory(Window owner, string path)
        {
            _saveToPath = path;
            ProgressWindow progressWindow = new ProgressWindow
            {
                ProgressMaximum = ImageShrinkerViewModel.SelectedImageCount,
                Title = "Im Verzeichnis speichern",
            };
            DoWorkerJob(progressWindow, SaveToDirectoryWorker);
        }

        private void PackToWorker(object s, DoWorkEventArgs args)
        {
            int saved = 0;
            int imagesToSave = ImageShrinkerViewModel.SelectedImageCount;

            string saveToPathName = Path.Combine(_saveToPath, ImageShrinkerViewModel.ArchiveName) + ".zip";
            if (File.Exists(saveToPathName))
                File.Delete(saveToPathName);

            using (ZipFile zip = new ZipFile())
            {
                try
                {
                    foreach (ImageThumbViewModel thumb in ImageShrinkerViewModel.ImageThumbs)
                    {
                        if (_worker.CancellationPending)
                        {
                            break;
                        }

                        if (thumb.IsSelected)
                        {
                            string progress = string.Format("Speichere Bild {0} von {1}", ++saved, imagesToSave);
                            _backgroundUI.UpdateDispatcher.BeginInvoke(new UpdateProgressDelegate(UpdateProgress), progress, saved);

                            thumb.SaveScaled((int)ImageShrinkerViewModel.DesiredSize.Width, TempPath,ImageShrinkerViewModel.Quality);
                            zip.AddFile(Path.Combine(TempPath, thumb.FileName), ImageShrinkerViewModel.ArchiveName);
                        }
                    }
                    if (!_worker.CancellationPending)
                    {
                        _backgroundUI.UpdateDispatcher.BeginInvoke(
                            new UpdateProgressDelegate(UpdateProgress), "Packe in Archiv ein...", ++saved);

                        FileStream saveToStream = new FileStream(saveToPathName, FileMode.Create);
                        zip.Save(saveToStream);
                        saveToStream.Close();
                    }
                }
                finally
                {
                    _backgroundUI.UpdateDispatcher.BeginInvoke(
                        new UpdateProgressDelegate(UpdateProgress), "Räume auf...", ++saved);
                    foreach (ImageThumbViewModel thumb in ImageShrinkerViewModel.ImageThumbs)
                    {
                        if (thumb.IsSelected)
                        {
                            string filetemp = Path.Combine(TempPath, thumb.FileName);
                            if (File.Exists(filetemp))
                            {
                                File.Delete(filetemp);
                            }
                        }
                    }
                }
            }
        }
        private void SaveToDirectoryWorker(object sender, DoWorkEventArgs args)
        {
            int saved = 0;
            int imagesToSave = ImageShrinkerViewModel.SelectedImageCount;

            string targetdir = Path.Combine(_saveToPath, ImageShrinkerViewModel.ArchiveName);
            Directory.CreateDirectory(targetdir);
            foreach (ImageThumbViewModel thumb in ImageShrinkerViewModel.ImageThumbs)
            {
                if (_worker.CancellationPending)
                    break;

                if (thumb.IsSelected)
                {
                    thumb.SaveScaled((int)ImageShrinkerViewModel.DesiredSize.Width, targetdir,ImageShrinkerViewModel.Quality);

                    string text = string.Format("Speichere Bild {0} von {1}", ++saved, imagesToSave);
                    _backgroundUI.UpdateDispatcher.BeginInvoke(
                        new UpdateProgressDelegate(UpdateProgress), text, saved);
                }
            }
        }

        private void LoadFromDirectoryWorker(object sender, DoWorkEventArgs args)
        {
            InternalAddImagesFromDirectory(_saveToPath);
        }

        private void InternalAddImagesFromDirectory(string absolutepath)
        {
            string[] directories = Directory.GetDirectories(absolutepath);
            foreach (string dir in directories)
            {
                if (_worker.CancellationPending) break;
                InternalAddImagesFromDirectory(dir);
            }

            string[] files = Directory.GetFiles(absolutepath);
            foreach (string file in files)
            {
                if (_worker.CancellationPending)
                {
                    ImageShrinkerViewModel.ImageThumbs.Clear();
                    break;
                }


                string extension = Path.GetExtension(file);
                if (!string.IsNullOrEmpty(extension) 
                    && (extension.ToUpper() == ".JPEG" || extension.ToUpper() == ".JPG"))
                {
                    _backgroundUI.UpdateDispatcher.BeginInvoke(
                        new UpdateProgressDelegate(UpdateProgress),
                        "Lade Bild " + Path.GetFileName(file) + "", 0);

                    ImageShrinkerViewModel.AddImage(file);
                }
            }

            ImageShrinkerViewModel.CalculateStatistics();
            ImageShrinkerViewModel.UpdateStatistics();
        }

        public void LoadFilesWorker(object sener, DoWorkEventArgs args)
        {
            foreach (string filename in _loadToFileNames)
            {
                if (_worker.CancellationPending)
                {
                    ImageShrinkerViewModel.ImageThumbs.Clear();
                    break;
                }
                ImageShrinkerViewModel.AddImage(filename);
            }
            ImageShrinkerViewModel.CalculateStatistics();
            ImageShrinkerViewModel.UpdateStatistics();
        }

        public delegate void UpdateProgressDelegate(string text, double progressValue);
        public void UpdateProgress(string text, double progressValue)
        {
            _backgroundUI.MessageText = text;
            _backgroundUI.ProgressValue = progressValue;
        }

        public delegate void IncreasingUpdateProgressDelegate(string text);
        public void IncreasingUpdateProgress(string text)
        {
            _backgroundUI.MessageText = text;
            _backgroundUI.ProgressValue += 1;
        }
    }
}