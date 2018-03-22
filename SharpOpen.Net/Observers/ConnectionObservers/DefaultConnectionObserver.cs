using System;
using System.Collections.Generic;
using System.Text;

namespace SharpOpen.Net.Observers.ConnectionObservers
{
    public abstract class DefaultConnectionObserver : IConnectionObserver
    {
        protected Server _Server;
        protected Connection _Client;

        public virtual string TraceFileName { get { return (_Server != null ? _Server.LocalPort.ToString() : _Client != null ? _Client.RemoteEndPoint.Address.ToString() : "TRACE") + " " + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt"; } }

        internal Server Server
        {
            set { _Server = value; }
        }

        public Connection Client
        {
            set { _Client = value; }
        }

        public abstract void HandleReceive(byte[] receiveBuffer, int offset, int count);
        public abstract void HandleLocalDisconnect();
        public abstract void HandleRemoteDisconnect();
    }
}
