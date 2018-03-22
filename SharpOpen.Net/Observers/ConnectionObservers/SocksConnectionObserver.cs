using System;
using System.Collections.Generic;
using System.Text;
using SharpOpen.Net.Socks;
using System.Net;

namespace SharpOpen.Net.Observers.ConnectionObservers
{
    public class SocksConnectionObserver : DefaultConnectionObserver
    {
        public override void HandleReceive(byte[] receiveBuffer, int offset, int count)
        {
            byte[] requestASCII = new byte[count];
            Array.ConstrainedCopy(receiveBuffer, offset, requestASCII, 0, count);
            Console.WriteLine("Socks request received: {0}\r\n{1}", Encoding.ASCII.GetString(requestASCII), Log.ByteArrayToHexString(requestASCII));
            
            Socks4Request request = Socks4Request.Parse(receiveBuffer);
            
            if (request == null)
            {
                Console.WriteLine("Could not parse socks request");
                SocksCommon.SendSocksError(_Client);
                return;
            }

            Connection requestedConnection = null;

            try
            {
                requestedConnection = Connection.Connect(request.RemoteHostIP, request.RemotePort);
                Console.WriteLine("{0} --> Socks Request successful, connected to {1}:{2}", _Client, request.RemoteHostIP, request.RemotePort);
            }
            catch
            {
                SocksCommon.SendSocksError(_Client);
                return;
            }

            requestedConnection.SetObserver(new TransmissionRedirectConnectionObserver(_Client, false));
            _Client.SetObserver(new TransmissionRedirectConnectionObserver(requestedConnection, true));

            // _Client will continue to receive, but destination needs to start receiveing.
            requestedConnection.BeginReceiving();

            SocksCommon.SendSocksSuccess(_Client, request.RemotePort, request.RemoteHostIP);

        }

        public override void HandleLocalDisconnect()
        {
            
        }

        public override void HandleRemoteDisconnect()
        {
            
        }
    }
}
