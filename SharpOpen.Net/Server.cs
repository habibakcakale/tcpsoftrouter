using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using SharpOpen.Net.Observers;

namespace SharpOpen.Net
{
    public class Server : Traceable
    {
        TcpListener _Server;
        IServerObserver _Observer;
        private bool _IsWaitingForConnections;
        private bool _IsBeingTraced;

        public Server() { }

        public static IPEndPoint GetAvailableLocalEndPoint()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0x0);
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(endPoint);
                IPEndPoint local = (IPEndPoint)socket.LocalEndPoint;
                return local;
            }
        }

        public static Server Start(int localPort)
        {
            return Start(IPAddress.Any, localPort);
        }

        public static Server Create(int localPort)
        {
            return Create(IPAddress.Any, localPort);
        }

        public static Server Create(IPAddress networkInterface, int localPort)
        {
            Server newInstance = Implementation.CreateServer();
            newInstance._Server = new TcpListener(networkInterface, localPort);
            return newInstance;
        }

        public static Server Start(IPAddress networkInterface, int localPort)
        {
            Server newServer = Create(networkInterface, localPort);
            newServer.Start();

            return newServer;
        }

        public IServerObserver Observer
        {
            get
            {
                return _Observer;
            }
        }

        public int LocalPort
        {
            get
            {
                return (_Server.LocalEndpoint as IPEndPoint).Port;
            }
        }

        public void Stop()
        {
            _IsWaitingForConnections = false;
            _Server.Stop();
        }

        public bool IsListening
        {
            get
            {
                return _IsWaitingForConnections;
            }
        }

        public void Start()
        {
            if (_IsWaitingForConnections) return;
            _Server.Start();
            _Server.BeginAcceptTcpClient(HandleConnectionRequest, null);
            _IsWaitingForConnections = true;
        }

        public void SetObserver(IServerObserver observer)
        {
            _Observer = observer;
            _Observer.ObservedServer = this;
        }

        protected virtual void OnConnectionAccepted(Connection acceptedConnection) { }

        private void HandleConnectionRequest(IAsyncResult asyncResult)
        {
            if (!_IsWaitingForConnections) return;

            Connection newConnection = null;

            try
            {
                newConnection = Connection.Accept(_Server, asyncResult);
                Performance.UpdateConnectoinAccepted();

                try
                {
                    OnConnectionAccepted(newConnection);
                }
                catch { }
            }
            catch
            {
                
            }

            try
            {
                _Server.BeginAcceptTcpClient(HandleConnectionRequest, null);
            }
            catch
            {
                return;
            }

            if (_Observer != null && newConnection != null)
            {
                try { _Observer.HandleConnectionRequest(newConnection); } catch { }
            }
        }
        
        public override bool IsBeingTraced
        {
            get
            {
                return _IsBeingTraced;
            }
            set
            {
                _IsBeingTraced = value;
            }
        }

    }
}
