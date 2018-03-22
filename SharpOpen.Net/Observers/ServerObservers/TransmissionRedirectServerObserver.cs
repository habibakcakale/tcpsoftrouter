using System;
using System.Collections.Generic;
using System.Text;
using SharpOpen.Net.Observers.ConnectionObservers;

namespace SharpOpen.Net.Observers.ServerObservers
{
    public class TransmissionRedirectServerObserver : DefaultServerObserver
    {
        protected string _RedirectHost;
        protected int _RedirectPort;

        public string RedirectHost { get { return _RedirectHost; } }
        public int RedirectPort { get { return _RedirectPort; } }

        public TransmissionRedirectServerObserver(string redirectHost, int redirectPort)
        {
            _RedirectHost = redirectHost;
            _RedirectPort = redirectPort;
        }

        public override void HandleConnectionRequest(Connection acceptedConnection)
        {
            Connection redirectConnection = null;
            try
            {
                redirectConnection = Connection.Connect(_RedirectHost, _RedirectPort);
                Console.WriteLine("Connected to Port Redirection Target {0}:{1}", _RedirectHost, _RedirectPort);
            }
            catch(Exception exp)
            {
                Console.WriteLine("Could Not Connect to Port Redirection Target {0}:{1}. Reason: {2}", _RedirectHost, _RedirectPort, exp);
                acceptedConnection.Close();
                return;
            }

            acceptedConnection.SetObserver(new TransmissionRedirectConnectionObserver(redirectConnection, true));
            redirectConnection.SetObserver(new TransmissionRedirectConnectionObserver(acceptedConnection, false));

            acceptedConnection.IsBeingTraced = ObservedServer.IsBeingTraced;

            acceptedConnection.BeginReceiving();
            redirectConnection.BeginReceiving();
        }
    }
}
