using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SharpOpen.Net.Observers.ConnectionObservers
{
    public class WebProxyConnectionObserver : DefaultConnectionObserver
    {
        private Connection _Destination;
        private bool _Success;
        private TunnelWaitHandle _TaskCompletedEvent;

        public bool Success { get { return _Success; } }

        public WebProxyConnectionObserver(Connection destination, TunnelWaitHandle taskCompletedEvent)
        {
            _Destination = destination;
            _TaskCompletedEvent = taskCompletedEvent;
        }

        public override void HandleReceive(byte[] receiveBuffer, int offset, int count)
        {
            string data = Encoding.ASCII.GetString(receiveBuffer);
            if (data.Contains("200 Connection established"))
            {
                Console.WriteLine("\r\nTunnel OK {0}", _Client);
                _Success = true;
                _TaskCompletedEvent.Result = _Client;
                _TaskCompletedEvent.IsSet = true;
                _Destination.SetObserver(null);
                _Destination.Close();
                _Destination.Send(Encoding.ASCII.GetBytes("HTTP/1.0 500 Internal Server Error\r\n\r\n"));
                _TaskCompletedEvent.Set();
                return;
            }

            if (!_Destination.Send(receiveBuffer, offset, count))
            {
                Console.WriteLine("\r\nError on Send: {0}", _Client);
                _Client.Close();
                _Destination.Close();
                _TaskCompletedEvent.IsSet = true;
                _TaskCompletedEvent.Set();
            }
        }

        public override void HandleLocalDisconnect()
        {
            _Destination.SetObserver(null);
            _Destination.Close();
        }

        public override void HandleRemoteDisconnect()
        {
            HandleLocalDisconnect();
        }
    }
}
