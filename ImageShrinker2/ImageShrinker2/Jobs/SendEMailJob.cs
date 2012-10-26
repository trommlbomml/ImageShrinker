
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Jobs
{
    class SendEMailJob : PackToDirectoryJob
    {
        private EMailSendViewModel _eMailSendViewModel;

        public SendEMailJob(EMailSendViewModel eMailSendViewModel) : base(string.Empty)
        {
            _eMailSendViewModel = eMailSendViewModel;
        }
    }
}
