using System;
using System.Collections.Generic;
using System.Text;
using SharpOpen.Net.Observers.ConnectionObservers;

namespace SharpOpen.Net.Observers.ServerObservers
{
    public class SocksOverTunnelServerObserver : DefaultServerObserver
    {
        private string _RemoteSocksServerHost;
        private int _RemoteSocksServerPort;

        public SocksOverTunnelServerObserver(string remoteSocksServerHost, int remoteSocksServerPort)
        {
            _RemoteSocksServerHost = remoteSocksServerHost;
            _RemoteSocksServerPort = remoteSocksServerPort;
        }

        public override void HandleConnectionRequest(Connection acceptedConnection)
        {
            Connection tunnelledConnection = null;

            try
            {
                Console.WriteLine("Trying To Connect To The Tunnel Socks Server");
                tunnelledConnection = Connection.ConnectOverHttpProxy(_RemoteSocksServerHost, _RemoteSocksServerPort);
                Console.WriteLine("Connect To The Tunnel Socks Server Success!");
            }
            catch
            {
                acceptedConnection.Close();
                return;
            }

            acceptedConnection.SetObserver(new TransmissionRedirectConnectionObserver(tunnelledConnection, true));
            tunnelledConnection.SetObserver(new TransmissionRedirectConnectionObserver(acceptedConnection, false));

            acceptedConnection.BeginReceiving();
            tunnelledConnection.BeginReceiving();
        }
    }
}
