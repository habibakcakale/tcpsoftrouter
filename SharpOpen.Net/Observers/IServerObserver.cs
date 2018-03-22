using System;
using System.Collections.Generic;
using System.Text;

namespace SharpOpen.Net.Observers
{
    public interface IServerObserver
    {
        Server ObservedServer { get; set; }
        void HandleConnectionRequest(Connection acceptedConnection);
    }
}
