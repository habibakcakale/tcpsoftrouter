using System;
using System.Collections.Generic;
using System.Text;
using SharpOpen.Net.Observers.ConnectionObservers;

namespace SharpOpen.Net.Observers.ServerObservers
{
    public class SocksServerObserver : DefaultServerObserver
    {
        public override void HandleConnectionRequest(Connection acceptedConnection)
        {
            Console.WriteLine("Accepted connection {0}", acceptedConnection);
            acceptedConnection.SetObserver(new SocksConnectionObserver());
            acceptedConnection.IsBeingTraced = ObservedServer.IsBeingTraced;
            acceptedConnection.BeginReceiving();
        }
    }
}
