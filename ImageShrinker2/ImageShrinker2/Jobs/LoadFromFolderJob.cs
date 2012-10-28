using System.ComponentModel;
using System.IO;
using ImageShrinker2.Framework;
using ImageShrinker2.Model;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Jobs
{
    class LoadFromFolderJob : AsyncJobBase
    {
        private readonly string _directory;

        public LoadFromFolderJob(string directory)
        {
            _directory = directory;
        }

        public override void Prepare(ImageShrinkerViewModel imageShrinkerViewModel, IBackgroundWorkerUi ui)
        {
            base.Prepare(imageShrinkerViewModel, ui);
            Ui.MessageText = "Loading Images...";
            Ui.IsIndeterminate = true;
        }

        public override void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            AddFromFolderInternal(_directory);
        }

        private void AddFromFolderInternal(string path)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                if (CancellationPending)
                    break;

                if (new FileInfo(file).Attributes == FileAttributes.Directory)
                {
                    AddFromFolderInternal(file);
                }
                else
                {
                    string extension = Path.GetExtension(file);
                    if (string.IsNullOrEmpty(extension)) continue;
                    extension = extension.ToLower();
                    if (extension == ".jpg" || extension == ".jpeg")
                    {
                        ImageViewModel imageViewModel = ImageModel.CreateFromFile(file);
                        InvokeIncreasingProgress(() => ImageShrinkerViewModel.AddImage(imageViewModel));
                    }
                }
            }
        }
    }
}
