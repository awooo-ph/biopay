using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using NORSU.BioPay.Properties;
using NORSU.BioPay.ViewModels;

namespace NORSU.BioPay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {

            awooo.IsRunning = true;
            awooo.Context = SynchronizationContext.Current;
            Scanner.Start();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();
            
            Scanner.Stop();
            base.OnExit(e);
        }
    }
}
