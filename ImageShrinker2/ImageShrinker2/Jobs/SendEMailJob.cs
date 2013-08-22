
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using ImageShrinker2.Framework;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Jobs
{
    class SendEMailJob : PackToDirectoryJob
    {
        private readonly EMailSendViewModel _eMailSendViewModel;
        private Exception _exception;

        public SendEMailJob(EMailSendViewModel eMailSendViewModel) : base(string.Empty)
        {
            _eMailSendViewModel = eMailSendViewModel;
            PackDirectory = CopyToDirectory;
        }

        public override void Prepare(ImageShrinkerViewModel imageShrinkerViewModel, IBackgroundWorkerUi ui)
        {
            base.Prepare(imageShrinkerViewModel, ui);
            Ui.ProgressMaximum += 2;
        }

        public override void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            var zipFile = Path.Combine(PackDirectory, ImageShrinkerViewModel.ArchiveName + ".zip");
            MailMessage msg = null;
            _exception = null;
            try
            {
                base.BackgroundWorkerOnDoWork(sender, e);

                IncreasingProgress("Versende E-Mail...");
                var smtpClient = new SmtpClient
                {
                    Host = _eMailSendViewModel.SelectedProvider.Smpt,
                    Credentials =
                        new NetworkCredential(_eMailSendViewModel.EMailAdress, _eMailSendViewModel.Password)
                };
                
                msg = new MailMessage(_eMailSendViewModel.EMailAdress, _eMailSendViewModel.Adressors, "Test", _eMailSendViewModel.Message);
                msg.Attachments.Add(new Attachment(zipFile));

                var sendReadyEvent = new AutoResetEvent(false);
                smtpClient.SendCompleted += SmtpClientOnSendCompleted;
                smtpClient.SendAsync(msg, sendReadyEvent);
                sendReadyEvent.WaitOne();

                if (_exception != null) throw _exception;
            }
            catch (Exception ex)
            {
                HandleError(ex, "Error occurred sending E-Mail", 
                                "E-Mail versenden fehlgeschlagen", 
                                string.Format("E-Mail konnte nicht versendet werden: {0}", ex.Message));
            }
            finally
            {
                if (msg != null) msg.Dispose();
                if (File.Exists(zipFile)) File.Delete(zipFile);
            }
            
        }

        private void SmtpClientOnSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                _exception = e.Error;

            ((AutoResetEvent) e.UserState).Set();
        }
    }
}
