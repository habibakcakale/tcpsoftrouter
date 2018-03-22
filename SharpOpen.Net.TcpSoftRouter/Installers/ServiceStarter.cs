using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;


namespace SharpOpen.Net.TcpSoftRouter
{
    [RunInstaller(true)]
    public partial class ServiceStarter : Installer
    {
        public ServiceStarter()
        {
            InitializeComponent();
        }

        private bool StartRequested
        {
            get
            {
                return Context.Parameters["start_service"] != null && Context.Parameters["start_service"] == "1";
            }
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);

            if (StartRequested)
            {
                ServiceController installedService = new ServiceController("SharpOpen.Net.TcpSoftRouter");
                installedService.Start();
            }
        }
    }
}
