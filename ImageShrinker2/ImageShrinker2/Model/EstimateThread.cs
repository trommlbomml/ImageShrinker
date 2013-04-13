using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ImageShrinker2.Framework;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Model
{
    class EstimateThread
    {
        private readonly ImageShrinkerViewModel _imageShrinkerViewModel;
        private bool _idle;
        private bool _doRestart;

        public bool Idle
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _idle; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { _idle = value; }
        }

        private bool DoRestart
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _doRestart; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { _doRestart = value; }
        }

        public void Stop()
        {
            Idle = true;
            DoRestart = true;
        }

        public void Start()
        {
            if (Idle)
            {
                Idle = false;
                DoRestart = false;
            }
            else
            {
                DoRestart = true;
            }
        }

        public IBackgroundWorkerUi Ui {get { return _imageShrinkerViewModel.Statusbar; }}

        public EstimateThread(ImageShrinkerViewModel imageShrinkerViewModel)
        {
            _imageShrinkerViewModel = imageShrinkerViewModel;
            Idle = true;
            var task = new Task(Process);
            task.Start();
        }

        private void Process()
        {
            while (true)
            {
                if (Idle) continue;
                var selectedImages = _imageShrinkerViewModel.Images.Where(i => i.IsSelected).ToList();
                
                Ui.ProgressMinimum = 1;
                Ui.ProgressMaximum = selectedImages.Count;
                Ui.ProgressValue = 1;
                Ui.MessageText = "Calculating Compressed Size";
                _imageShrinkerViewModel.CompressedSize = 0;
                Ui.AfterAsyncStart();
                
                _sizeSum = 0;
                Parallel.ForEach(selectedImages, CalculateImage);

                if (DoRestart)
                {
                    DoRestart = false;
                    continue;
                }

                Ui.MessageText = "Ready.";
                _imageShrinkerViewModel.CompressedSize = _sizeSum;
                Idle = true;
                Ui.OnWorkerCompleted();
            }
        }

        private readonly object _lockObject = new object();
        private double _sizeSum;

        private void CalculateImage(ImageViewModel imageViewModel, ParallelLoopState state, Int64 counter)
        {
            if (DoRestart) state.Stop();

            var messageText = "Calculating Compressed Size" +
                              new string('.', (int)(counter % 4));

            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action)(() =>
                {
                    Ui.ProgressValue += 1;
                    Ui.MessageText = messageText;
                }));

            lock (_lockObject)
            {
                _sizeSum += ImageModel.GetFileSizeScaledInMegaByte(imageViewModel);
            }
        }
    }
}
