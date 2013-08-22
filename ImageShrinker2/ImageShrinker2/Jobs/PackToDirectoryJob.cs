
using System.IO;
using Ionic.Zip;

namespace ImageShrinker2.Jobs
{
    class PackToDirectoryJob : CopyToFolderJob
    {
        protected string PackDirectory;

        public PackToDirectoryJob(string directory) : base(string.Empty)
        {
            PackDirectory = directory;
            CopyToDirectory = Path.Combine(Path.GetTempPath(), "ImageShrinker");
        }

        public override void BackgroundWorkerOnDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            base.BackgroundWorkerOnDoWork(sender, e);
            string tempPathOfImages = Path.Combine(CopyToDirectory, ImageShrinkerViewModel.ArchiveName);
            try
            {
                IncreasingProgress("Erstelle komprimiertes Archiv...");
                using (var zipFile = new ZipFile())
                {
                    foreach (string file in Directory.GetFiles(tempPathOfImages))
                    {
                        zipFile.AddFile(file, ImageShrinkerViewModel.ArchiveName);
                    }
                    zipFile.Save(Path.Combine(PackDirectory, ImageShrinkerViewModel.ArchiveName + ".zip"));
                }
            }
            finally
            {
                if (Directory.Exists(tempPathOfImages))
                {
                    IncreasingProgress("Tempverzeichnisse aufräumen...");
                    foreach (var file in Directory.GetFiles(tempPathOfImages)) File.Delete(file);
                    Directory.Delete(tempPathOfImages);
                }
            }
        }

        public override void Prepare(ViewModels.ImageShrinkerViewModel imageShrinkerViewModel, Framework.IBackgroundWorkerUi ui)
        {
            base.Prepare(imageShrinkerViewModel, ui);
            Ui.ProgressMaximum += 2;
        }
    }
}
