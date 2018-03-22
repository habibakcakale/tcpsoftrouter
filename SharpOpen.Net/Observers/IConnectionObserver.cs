using System;
using System.Collections.Generic;
using System.Text;

namespace SharpOpen.Net.Observers
{
    public interface IConnectionObserver
    {
        Connection Client { set; }
        void HandleReceive(byte[] receiveBuffer, int offset, int count);
        void HandleLocalDisconnect();
        void HandleRemoteDisconnect();
        string TraceFileName { get; }
    }
}
