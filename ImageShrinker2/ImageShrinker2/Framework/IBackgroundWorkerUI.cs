
using System.ComponentModel;

namespace ImageShrinker2.Framework
{
    interface IBackgroundWorkerUi
    {
        string MessageText { get; set; }
        double ProgressMinimum { get; set; }
        double ProgressMaximum { get; set; }
        double ProgressValue { get; set; }
        bool IsIndeterminate { get; set; }
        void OnWorkerCompleted();
        void AfterAsyncStart();
        BackgroundWorker Worker { get; set; }
    }
}
