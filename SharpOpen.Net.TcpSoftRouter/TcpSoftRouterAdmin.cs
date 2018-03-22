using System;
using System.Collections.Generic;
using System.Text;
using SharpOpen.Net.Observers.ServerObservers;
using SharpOpen.Net.Observers.ConnectionObservers;
using System.Net;
using System.Xml;
using System.Xml.Xsl;
using System.IO;

namespace SharpOpen.Net.TcpSoftRouter
{
    internal class AdminServerObserver : DefaultServerObserver
    {
        public override void HandleConnectionRequest(Connection acceptedConnection)
        {
            acceptedConnection.SetObserver(new AdminConnectionObserver());
            AdminConnectionObserver.SendCommandPrefix(acceptedConnection);
            acceptedConnection.BeginReceiving();
        }
    }

    internal class AdminConnectionObserver : BufferedConnectionObserver
    {
        private void Send(string text)
        {
            _Client.Send(Encoding.ASCII.GetBytes(text + "\r\n"));
        }

        private void Send(string textFormat, params object[] arguments)
        {
            Send(string.Format(textFormat, arguments));
        }

        internal AdminConnectionObserver()
        {
            AddTest(new byte[] { 0x0D, 0x0A });
            AddTest(new byte[] { 0x0A });
            base.OnRequestCompleted += new OnRequestCompletedDelegate(AdminConnectionObserver_OnRequestCompleted);
        }

        private int TestPort(string portString)
        {
            int port;
            if (!int.TryParse(portString, out port))
            {
                Send("500 Syntax Error: Port " + portString + " is not a valid integer");
                SendCommandPrefix(_Client);
            }
            if (port > System.Net.IPEndPoint.MaxPort || port < System.Net.IPEndPoint.MinPort)
            {
                Send("501 Overflow: Port " + port + " is out of range of valid port range");
                SendCommandPrefix(_Client);
                port = 0;
            }
            return port;
        }

        private bool TestLocalPort(string portString, out int localPort)
        {
            localPort = TestPort(portString);
            if (localPort == 0) return false;
            if (Servers.Instance.Contains(localPort))
            {
                Send("502 The local port " + portString + " is already in use");
                SendCommandPrefix(_Client);
                return false;
            }
            return true;
        }

        void AdminConnectionObserver_OnRequestCompleted(Connection client, int matchingTestIndex, byte[] buffer)
        {
            Send("");

            string[] textCommand = Encoding.ASCII.GetString(buffer, 0, buffer.Length - (matchingTestIndex == 0 ? 2 : 1)).ToUpperInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            XmlDocument response = Processor.Execute(textCommand);

            // Send(response.DocumentElement.Attributes["statuscode"].Value + " " + response.DocumentElement.Attributes["statusmessage"].Value);

            MemoryStream responseStream = new MemoryStream();
            TcpSoftRouter.telnetTransform.Transform(response, null, responseStream);
            _Client.Send(responseStream.ToArray(), 3, Convert.ToInt32(responseStream.Length - 3));

            if (textCommand[0] == "EXIT")
            {
                _Client.Close();
            }
            else
            {
                SendCommandPrefix(_Client);
            }
        }

        internal static void SendCommandPrefix(Connection client)
        {
            if (client.IsConnected)
            {
                client.Send(Encoding.ASCII.GetBytes("\r\nSharpOpen.Net.TcpSoftRouter -> "));
            }
        }




    }
}
