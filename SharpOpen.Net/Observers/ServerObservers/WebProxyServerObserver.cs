using System;
using System.Collections.Generic;
using System.Text;
using SharpOpen.Net.Observers.ConnectionObservers;
using System.Threading;

namespace SharpOpen.Net.Observers.ServerObservers
{
    public class WebProxyServerObserver : DefaultServerObserver
    {
        private TunnelWaitHandle _TaskCompletedEvent;

        public WebProxyServerObserver(TunnelWaitHandle taskCompletedEvent)
        {
            _TaskCompletedEvent = taskCompletedEvent;
        }

        public override void HandleConnectionRequest(Connection acceptedConnection)
        {
            if (_TaskCompletedEvent.IsSet)
            {
                acceptedConnection.Close();
                return;
            }

            if(!HttpProxy.Exists)
            {
                acceptedConnection.Close();
                return;
            }

            Console.WriteLine("\r\nAccepted {0}", acceptedConnection);

            Connection realProxyConnection = null;
            try
            {
                realProxyConnection = Connection.Connect(HttpProxy.InnerProxy.Address.Host, HttpProxy.InnerProxy.Address.Port);
                Console.WriteLine("\r\nRouted Over {0}", realProxyConnection);
            }
            catch
            {
                acceptedConnection.Close();
                return;
            }

            WebProxyConnectionObserver webProxyObserver = new WebProxyConnectionObserver(acceptedConnection, _TaskCompletedEvent);

            acceptedConnection.SetObserver(new TransmissionRedirectConnectionObserver(realProxyConnection, true));
            realProxyConnection.SetObserver(webProxyObserver);

            acceptedConnection.BeginReceiving();
            realProxyConnection.BeginReceiving();
        }
    }
}
