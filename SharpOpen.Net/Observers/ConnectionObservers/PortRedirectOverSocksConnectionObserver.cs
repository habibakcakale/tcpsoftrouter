using System;
using System.Collections.Generic;
using System.Text;
using SharpOpen.Net.Socks;

namespace SharpOpen.Net.Observers.ConnectionObservers
{
    public class PortRedirectOverSocksConnectionObserver : DefaultConnectionObserver
    {
        private Connection _ClientConnection;

        public PortRedirectOverSocksConnectionObserver(Connection clientConnection)
        {
            _ClientConnection = clientConnection;
            // _Client is the socks server connection initiated by the PortRedirectOverSocksServerObserver instance
            // _ClientConnection is the real client connection initiated the whole process
        }

        public override void HandleReceive(byte[] receiveBuffer, int offset, int count)
        {
            try
            {
                Performance.UpdateBytesReceived(count);

                Socks4Response socksServerResponse = Socks4Response.Parse(receiveBuffer);
                if (!socksServerResponse.Success)
                {
                    HandleLocalDisconnect();
                    return;
                }

                _ClientConnection.SetObserver(new TransmissionRedirectConnectionObserver(_Client, true));
                _Client.SetObserver(new TransmissionRedirectConnectionObserver(_ClientConnection, false));

                _ClientConnection.BeginReceiving();
            }
            catch (Exception exp)
            {
                HandleLocalDisconnect();
            }
        }

        public override void HandleLocalDisconnect()
        {
            _ClientConnection.Close();
            _Client.Close();
        }

        public override void HandleRemoteDisconnect()
        {
            HandleLocalDisconnect();
        }
    }
}
