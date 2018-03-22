using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using SharpOpen.Net.Observers;
using SharpOpen.Net.Observers.ServerObservers;
using System.Threading;
using System.IO;

namespace SharpOpen.Net
{
    public class Connection : Traceable
    {
        private TcpClient _Client;
        private byte[] _ReceiveBuffer;
        private IPEndPoint _LocalEndPoint;
        private IPEndPoint _RemoteEndPoint;
        private bool _IsConnected;
        private bool _IsReceiving;
        private bool _IsWaitingToReceive;
        private IConnectionObserver _Observer;

        private static List<Connection> _RunningInstances = new List<Connection>();

        public const int _ReceiveBufferSize = 0x2000;

        public Connection()
        {

        }

        protected virtual void OnInstanceInitialized() { }

        private static void InitializeConnection(Connection newInstance, TcpClient newClient)
        {
            newInstance._Client = newClient;
            newInstance._IsConnected = true;
            newInstance._IsReceiving = false;
            newInstance._LocalEndPoint = newClient.Client.LocalEndPoint as IPEndPoint;
            newInstance._RemoteEndPoint = newClient.Client.RemoteEndPoint as IPEndPoint;
            newInstance._ReceiveBuffer = new byte[_ReceiveBufferSize];
            _RunningInstances.Add(newInstance);

            try
            {
                newInstance.OnInstanceInitialized();
            }
            catch { }
        }

        public static Connection Accept(TcpListener server, IAsyncResult asyncResult)
        {
            TcpClient newClient = server.EndAcceptTcpClient(asyncResult);
            Connection newInstance = Implementation.CreateConnection();
            InitializeConnection(newInstance, newClient);
            return newInstance;
        }

        public static Connection ConnectOverHttpProxy(IPAddress remoteHost, int remotePort)
        {
            return ConnectOverHttpProxy(remoteHost.ToString(), remotePort);
        }

        public static Connection ConnectOverHttpProxy(string remoteHost, int remotePort)
        {
            if (!HttpProxy.Exists) throw new InvalidOperationException("Http Proxy has not been set.");

            int availableLocalPort = Server.GetAvailableLocalEndPoint().Port;

            Server fakeProxy = Server.Start(IPAddress.Parse("127.0.0.1"), availableLocalPort);

            Console.WriteLine("\r\nConnectOverHttpProxy: Fake server listening on port {0}", availableLocalPort);

            TunnelWaitHandle taskCompletedEvent = new TunnelWaitHandle();

            fakeProxy.SetObserver(new WebProxyServerObserver(taskCompletedEvent));

            System.Net.HttpWebRequest webRequest = HttpWebRequest.Create("https://" + remoteHost + ":" + remotePort) as HttpWebRequest;
            webRequest.Proxy = HttpProxy.GenerateLocalProxy(availableLocalPort);
            webRequest.KeepAlive = false;

            webRequest.BeginGetResponse(null, null);

            taskCompletedEvent.WaitOne();

            webRequest.Abort();

            webRequest = null;

            fakeProxy.Stop();

            if (taskCompletedEvent.Result == null)
            {
                throw new System.Exception("Could not connect to remote host");
            }

            Console.WriteLine("nConnectOverHttpProxy: Connected to external net!");

            return taskCompletedEvent.Result as Connection;
        }

        public static Connection Connect(IPAddress remoteHost, int remotePort)
        {
            return Connect(remoteHost.ToString(), remotePort);
        }

        public static Connection Connect(string remoteHost, int remotePort)
        {
            TcpClient newClient = new TcpClient();
            newClient.Connect(remoteHost, remotePort);
            Connection newInstance = Implementation.CreateConnection();
            InitializeConnection(newInstance, newClient);
            return newInstance;
        }

        public override string TraceFileName
        {
            get
            {
                if (_Observer != null) return _Observer.TraceFileName;
                return this.RemoteEndPoint.Address + "_" + this.RemoteEndPoint.Port + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt";
            }
        }

        public bool IsConnected
        {
            get { return _IsConnected; }
        }

        public void Close()
        {
            Console.WriteLine("\r\nConnection.Close invoked by client code: {0}", this);
            if (!_IsConnected) return;
            _IsConnected = false;
            Close(true);
        }

        public IPEndPoint LocalEndPoint { get { return _LocalEndPoint; } }

        public IPEndPoint RemoteEndPoint { get { return _RemoteEndPoint; } }

        protected virtual void OnConnectionClosed() { }

        private void Close(bool notifyObserver)
        {
            _RunningInstances.Remove(this);

            if (_Client == null) return;

            try
            {
                OnConnectionClosed();
            }
            catch { }

            Console.WriteLine("{0} --> Close(bool) Invoked with values {1}", this, notifyObserver);

            try { _Client.GetStream().Dispose(); }
            catch { }
            try { _Client.GetStream().Close(); }
            catch { }
            try { _Client.Client.Close(); }
            catch { }
            try { _Client.Close(); }
            catch { }

            _Client = null;

            IsBeingTraced = false;

            if (notifyObserver && _Observer != null) _Observer.HandleLocalDisconnect();
        }

        public void BeginReceiving()
        {
            Console.WriteLine("{0} --> BeginReceiving() Invoked", this);
            if (!_IsConnected) return;
            if (_IsReceiving) return;
            if (_IsWaitingToReceive) return;

            _IsReceiving = true;
            BeginReceivingInternal();
        }

        private void BeginReceivingInternal()
        {
            Console.WriteLine("{0} --> BeginReceivingInternal() Invoked", this);
            if (!_IsConnected) return;
            if (!_IsReceiving) return;
            if (_IsWaitingToReceive) return;

            try
            {
                _Client.GetStream().BeginRead(_ReceiveBuffer, 0, _ReceiveBufferSize, HandleReceive, null);
                _IsWaitingToReceive = true;
            }
            catch
            {
                _IsReceiving = false;
                _IsWaitingToReceive = false;
                Close(false);
            }
        }

        public void StopReceiving()
        {
            Console.WriteLine("{0} --> StopReceiving() Invoked", this);
            if (!_IsConnected) return;
            if (!_IsReceiving) return;
            _IsReceiving = false;
        }

        public bool Send(byte[] buffer)
        {
            if (buffer == null) return false;
            return Send(buffer, buffer.Length);
        }

        public bool Send(byte[] buffer, int size)
        {
            return Send(buffer, 0, size);
        }

        protected virtual void OnDataSent(byte[] buffer, int offset, int count) { }

        public bool Send(byte[] buffer, int offset, int count)
        {
            if (!_IsConnected) return false;

            try
            {
                _Client.GetStream().Write(buffer, offset, count);

                try
                {
                    OnDataSent(buffer, offset, count);
                }
                catch { }

                Trace(buffer, offset, count);

                Console.WriteLine("{0} --> Sent {1} bytes of data to destination", this, count);
            }
            catch (Exception exp)
            {
                Console.WriteLine("{0} --> Send() Failed with: \r\n\r\n {1}", this, exp);
                return false;
            }
            return true;
        }

        public void SetObserver(IConnectionObserver observer)
        {
            _Observer = observer;
            if (_Observer != null) _Observer.Client = this;
        }

        protected virtual void OnDataReceived(byte[] data, int offset, int count) { }

        private void HandleReceive(IAsyncResult asyncResult)
        {
            if (!_IsConnected) return;

            _IsWaitingToReceive = false;

            int readBytes = 0;

            try { readBytes = _Client.GetStream().EndRead(asyncResult); }
            catch { }

            if (readBytes == 0)
            {
                Console.WriteLine(" *** CLOSE *** Read Bytes = 0: {0}", this);

                if (_Observer != null)
                {
                    try { _Observer.HandleRemoteDisconnect(); }
                    catch { }
                }

                Close(false);
                return;
            }

            Console.WriteLine(" *** RCV *** Read Bytes = {0} Connection: {1}", readBytes, this);

            try
            {
                OnDataReceived(_ReceiveBuffer, 0, readBytes);
            }
            catch { }

            if (_Observer != null)
            {
                try { _Observer.HandleReceive(_ReceiveBuffer, 0, readBytes); }
                catch { }
            }

            Trace(_ReceiveBuffer, 0, readBytes);

            BeginReceivingInternal();

        }

        public void BeginReceivingSync()
        {
            if (!_IsConnected || _IsReceiving || _IsWaitingToReceive) throw new InvalidOperationException("Synchronized receive cannot be started on disconnected sockets or while asynchronous receive is in progress");

            _IsReceiving = true;
            _IsWaitingToReceive = true;
        }

        public byte[] GetNextSyncReceiveBuffer()
        {
            int readBytes = 0;
            try
            {
                readBytes = _Client.GetStream().Read(_ReceiveBuffer, 0, _ReceiveBufferSize);
            }
            catch
            {
                Close();
                return null;
            }
            byte[] clientBuffer = new byte[readBytes];
            Array.ConstrainedCopy(_ReceiveBuffer, 0, clientBuffer, 0, readBytes);
            return clientBuffer;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", _LocalEndPoint, _RemoteEndPoint);
        }

        ~Connection()
        {
            Close(false);
        }


    }
}
