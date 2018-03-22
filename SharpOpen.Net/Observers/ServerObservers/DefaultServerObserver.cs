using System;
using System.Collections.Generic;
using System.Text;

namespace SharpOpen.Net.Observers.ServerObservers
{
    public abstract class DefaultServerObserver : IServerObserver
    {
        public Server _ObservedServer;

        public abstract void HandleConnectionRequest(Connection acceptedConnection);

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

        // public event ConnectionAccepted;
    }
}
