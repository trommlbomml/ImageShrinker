
using System.Windows;
using log4net.Config;

namespace ImageShrinker2
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            XmlConfigurator.Configure();
        }
    }
}
