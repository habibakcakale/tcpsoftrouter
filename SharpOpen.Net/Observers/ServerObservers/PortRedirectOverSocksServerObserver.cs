using System;
using System.Collections.Generic;
using System.Text;
using SharpOpen.Net.Socks;
using SharpOpen.Net.Observers.ConnectionObservers;

namespace SharpOpen.Net.Observers.ServerObservers
{
    public class PortRedirectOverSocksServerObserver : TransmissionRedirectServerObserver
    {
        protected string _SocksServerHost;
        protected int _SocksServerPort;

        public string SocksServerHost { get { return _SocksServerHost; } }
        public int SocksServerPort { get { return _SocksServerPort; } }

        public PortRedirectOverSocksServerObserver(string socksServerHost, int socksServerPort, string redirectHost, int redirectPort)
            : base(redirectHost, redirectPort)
        {
            _SocksServerHost = socksServerHost;
            _SocksServerPort = socksServerPort;
        }

        public override void HandleConnectionRequest(Connection acceptedConnection)
        {
            Socks4Request request = Socks4Request.Build(Socks4Request.REQUEST_TYPE_BIND, _RedirectHost, _RedirectPort, "");

            Connection redirectConnection = null;
            try
            {
                redirectConnection = Connection.Connect(_SocksServerHost, _SocksServerPort);
            }
            catch(Exception exp)
            {
                acceptedConnection.Close();
                return;
            }

            redirectConnection.SetObserver(new PortRedirectOverSocksConnectionObserver(acceptedConnection));
            acceptedConnection.IsBeingTraced = ObservedServer.IsBeingTraced;
            redirectConnection.BeginReceiving();
            
            redirectConnection.Send(request.ToByteArray());
        }
    }
}
