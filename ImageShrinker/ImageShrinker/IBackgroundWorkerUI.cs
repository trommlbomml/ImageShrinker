
using System.ComponentModel;
using System.Windows.Threading;
namespace ImageShrinker
{
    interface IBackgroundWorkerUI
    {
        bool Aborting { get; set; }
        string MessageText { get; set; }
        double ProgressMinimum { get; set; }
        double ProgressMaximum { get; set; }
        double ProgressValue { get; set; }
        bool IsIndeterminate { get; set; }
        void OnWorkerCompleted();
        void AfterAsyncStart();
        BackgroundWorker Worker { get; set; }
        Dispatcher UpdateDispatcher { get; }
    }
}
