using System;
using System.Collections.Generic;
using System.Text;

namespace SharpOpen.Net.Observers.ConnectionObservers
{
    public class TransmissionRedirectConnectionObserver : DefaultConnectionObserver
    {
        protected Connection _Destination;
        protected bool _IsClientToServer;

        public TransmissionRedirectConnectionObserver(Connection destination, bool isClientToServer)
        {
            _Destination = destination;
            _IsClientToServer = isClientToServer;
        }

        public override string TraceFileName
        {
            get
            {
                return _Client.RemoteEndPoint.Address + " " +_Client.RemoteEndPoint.Port + " to " + _Destination.RemoteEndPoint.Address + " " + _Destination.RemoteEndPoint.Port + " at " + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt";
            }
        }

        public override void HandleReceive(byte[] receiveBuffer, int offset, int count)
        {
            if (_IsClientToServer)
            {
                Performance.UpdateBytesSent(count);                
            }
            else
            {
                Performance.UpdateBytesReceived(count);
            }

            if (!_Destination.Send(receiveBuffer, offset, count))
            {
                _Client.Close();
            }
        }

        public override void HandleLocalDisconnect()
        {
            _Destination.Close();
        }

        public override void HandleRemoteDisconnect()
        {
            _Destination.Close();
        }
    }
}
