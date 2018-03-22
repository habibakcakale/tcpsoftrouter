using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Diagnostics;
using System.IO;

namespace SharpOpen.Net.TcpSoftRouter
{
    [RunInstaller(true)]
    public partial class TcpSoftRouterInstaller : ServiceProcessInstaller
    {
        public TcpSoftRouterInstaller()
        {
            InitializeComponent();

            ServiceInstaller serviceInstaller = new ServiceInstaller();

            serviceInstaller.ServiceName = "SharpOpen.Net.TcpSoftRouter";
            serviceInstaller.DisplayName = "SharpOpen.Net.TcpSoftRouter";
            serviceInstaller.Description = "Routes Tcp transmission from local ports to defined target ports on target servers";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            Account = ServiceAccount.LocalSystem;

            Installers.Add(serviceInstaller);

            Installers.Add(new PerformanceCounterInstallProvider());
        }

    }
}