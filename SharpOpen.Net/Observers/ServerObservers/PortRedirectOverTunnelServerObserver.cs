using System;
using System.Collections.Generic;
using System.Text;
using SharpOpen.Net.Socks;
using System.Threading;
using SharpOpen.Net.Observers.ConnectionObservers;
using System.Net;

namespace SharpOpen.Net.Observers.ServerObservers
{
    public class PortRedirectOverTunnelServerObserver : DefaultConnectionObserver, IServerObserver
    {
        private string _RemoteSocksServerHost;
        private int _RemoteSocksServerPort;
        private string _RedirectHost;
        private int _RedirectPort;
        private AutoResetEvent _WaitHandle;
        private Socks4Response _RemoteSocksServerResponse;
        public Server _ObservedServer;

        public PortRedirectOverTunnelServerObserver(string remoteSocksServerHost, int remoteSocksServerPort, string redirectHost, int redirectPort)
        {
            _RemoteSocksServerHost = remoteSocksServerHost;
            _RemoteSocksServerPort = remoteSocksServerPort;
            _RedirectHost = redirectHost;
            _RedirectPort = redirectPort;
            _WaitHandle = new AutoResetEvent(false);
        }

        public void HandleConnectionRequest(Connection acceptedConnection)
        {
            Connection remoteSocksServerConnection = null;
            try
            {
                remoteSocksServerConnection = Connection.ConnectOverHttpProxy(_RemoteSocksServerHost, _RemoteSocksServerPort);
            }
            catch
            {
                acceptedConnection.Close();
                return;
            }

            remoteSocksServerConnection.SetObserver(this);

            remoteSocksServerConnection.Send(Socks4Request.Build(Socks4Request.REQUEST_TYPE_CONNECT, _RedirectHost, _RedirectPort, "").ToByteArray());

            _WaitHandle.WaitOne();

            if (_RemoteSocksServerResponse == null || !_RemoteSocksServerResponse.Success)
            {
                SocksCommon.SendSocksError(acceptedConnection);
                return;
            }

            SocksCommon.SendSocksSuccess(acceptedConnection, _RemoteSocksServerResponse.Port, IPAddress.Parse(_RemoteSocksServerResponse.ResolvedIP));

            acceptedConnection.SetObserver(new TransmissionRedirectConnectionObserver(remoteSocksServerConnection, true));
            remoteSocksServerConnection.SetObserver(new TransmissionRedirectConnectionObserver(acceptedConnection, false));
            acceptedConnection.BeginReceiving();
        }

        public override void HandleReceive(byte[] receiveBuffer, int offset, int count)
        {
            try
            {
                _RemoteSocksServerResponse = Socks4Response.Parse(receiveBuffer);
            }
            catch
            {
            }
            _WaitHandle.Set();
        }

        public override void HandleLocalDisconnect()
        {
            _WaitHandle.Set();
        }

        public override void HandleRemoteDisconnect()
        {
            _WaitHandle.Set();
        }

        public Server ObservedServer
        {
            get
            {
                return _ObservedServer;
            }
            set
            {
                _ObservedServer = value;
            }
        }
    }
}
