using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System;
using System.Threading;
using System.Diagnostics;
using SharpOpen.Net.TcpSoftRouter.Implementations;

namespace SharpOpen.Net.TcpSoftRouter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Implementation.SetConnectionImplementation(typeof(ConnectionImp));
            Implementation.SetServerImplementation(typeof(ServerImp));

#if DEBUG
            TcpSoftRouter softRouter = new TcpSoftRouter();
            softRouter.CallOnStart();

            while (true) Thread.Sleep(1000);
#else
            ServiceBase[] ServicesToRun;

            ServicesToRun = new ServiceBase[] { new TcpSoftRouter() };

            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}