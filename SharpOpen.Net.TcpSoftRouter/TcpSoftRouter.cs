using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.IO;
using System.Net;
using System.Xml.Xsl;
using System.Xml;

namespace SharpOpen.Net.TcpSoftRouter
{
    partial class TcpSoftRouter : ServiceBase
    {
        private Server administrationServer;
        private HttpListener webAdministrationServer;
        internal static string allowedAccess = ConfigurationManager.AppSettings["administrationaccess"];
        internal static XslCompiledTransform telnetTransform = new XslCompiledTransform();
        internal static XslCompiledTransform htmlTransform = new XslCompiledTransform();
        public TcpSoftRouter()
        {
            InitializeComponent();
        }
#if DEBUG
        internal void CallOnStart()
        {
            OnStart(null);
        }
#endif
        protected override void OnStart(string[] args)
        {
            MemoryStream stream = new MemoryStream();
            byte[] bytes = Encoding.ASCII.GetBytes(TcpSoftRouterResources.telnet);
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            telnetTransform.Load(XmlReader.Create(stream));

            stream = new MemoryStream();
            bytes = Encoding.ASCII.GetBytes(TcpSoftRouterResources.html);
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            htmlTransform.Load(XmlReader.Create(stream));

            int telnetAdministrationPort;
            if (!int.TryParse(ConfigurationManager.AppSettings["telnetadministrationport"], out telnetAdministrationPort))
            {
                telnetAdministrationPort = 17000;
            }
            try
            {
                administrationServer = Server.Start(telnetAdministrationPort);
                administrationServer.SetObserver(new AdminServerObserver());
            }
            catch (Exception exp)
            {
                EventLog.WriteEntry("SharpOpen.Net.TcpSoftRouter", "The service administration cannot be started on local port " + telnetAdministrationPort + ": " + exp, EventLogEntryType.Error);
            }

            if (HttpListener.IsSupported)
            {
                int webAdministrationPort;
                if (!int.TryParse(ConfigurationManager.AppSettings["webadministrationport"], out webAdministrationPort))
                {
                    webAdministrationPort = 17001;
                }
                try
                {
                    webAdministrationServer = new HttpListener();
                    webAdministrationServer.Prefixes.Add("http://*:" + webAdministrationPort + "/");
                    webAdministrationServer.Start();
                    webAdministrationServer.BeginGetContext(TcpSoftRouterWebAdmin.HandleWebAdministrationRequest, webAdministrationServer);
                }
                catch (Exception exp)
                {
                    EventLog.WriteEntry("SharpOpen.Net.TcpSoftRouter", "The service web administration cannot be started on local port " + webAdministrationPort + ": " + exp, EventLogEntryType.Error);
                }
            }

            Servers.LoadStorage();
        }

        protected override void OnStop()
        {
            if (administrationServer != null)
            {
                try
                {
                    administrationServer.Stop();
                    administrationServer = null;
                }
                catch { }
            }
        }
    }
}
