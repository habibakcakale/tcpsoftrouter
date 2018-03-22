using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SharpOpen.Net.Socks
{
    public class Socks4Request
    {
        public const byte REQUEST_TYPE_CONNECT = 0x01;
        public const byte REQUEST_TYPE_BIND = 0x02;

        public byte RequestType;
        public string Version = "4";
        public int RemotePort;
        public string UserID;
        public IPAddress RemoteHostIP;
        public string RemoteHost;

        private Socks4Request() { }

        private Socks4Request(string version, byte requestType, IPAddress remoteHostIP, int remotePort, string userID, string remoteHost)
        {
            RequestType = requestType;
            Version = version;
            RemoteHost = remoteHost;
            RemoteHostIP = remoteHostIP;
            RemotePort = remotePort;
            UserID = userID;
        }

        public static Socks4Request Build(byte requestType, IPAddress remoteHostIP, int remotePort, string userID)
        {
            return new Socks4Request("4", requestType, remoteHostIP, remotePort, userID, "");
        }

        public static Socks4Request Build(byte requestType, string remoteHost, int remotePort, string userID)
        {
            return new Socks4Request("4a", requestType, IPAddress.Parse("0.0.0.1"), remotePort, userID, remoteHost);
        }

        public byte[] ToByteArray()
        {
            List<byte> socks4Request = new List<byte>();

            byte[] ipAddress = new byte[4];
            int i = 0;
            foreach (string ipPart in RemoteHostIP.ToString().Split('.')) ipAddress[i++] = byte.Parse(ipPart);

            socks4Request.AddRange(new byte[] { 0x04, RequestType, (byte)(RemotePort >> 8), (byte)(RemotePort & 0XFF), ipAddress[0], ipAddress[1], ipAddress[2], ipAddress[3] });

            byte[] userID = Encoding.ASCII.GetBytes(UserID);
            socks4Request.AddRange(userID);
            socks4Request.Add(0x00);

            if (Version == "4a")
            {
                byte[] remoteHost = Encoding.ASCII.GetBytes(RemoteHost);
                socks4Request.AddRange(remoteHost);
                socks4Request.Add(0x00);
            }

            return socks4Request.ToArray();
        }

        public static Socks4Request Parse(byte[] socks4RequestData)
        {
            return Parse(socks4RequestData, true);
        }

        public static Socks4Request Parse(byte[] socks4RequestData, bool tryToResolveHost)
        {
            Socks4Request request = new Socks4Request();

            if (socks4RequestData[0] != 0x04) return null; // Only Socks 4 and 4a request are supported
            if (socks4RequestData[1] != Socks4Request.REQUEST_TYPE_CONNECT && socks4RequestData[1] != Socks4Request.REQUEST_TYPE_BIND) return null;

            request.RequestType = socks4RequestData[1];

            request.RemotePort = socks4RequestData[2] * 0x100 + socks4RequestData[3];

            byte[] ipAddress = new byte[4];
            Array.ConstrainedCopy(socks4RequestData, 4, ipAddress, 0, 4);

            int userIDEndIndex;
            request.UserID = SocksCommon.ReadNullTerminatedString(socks4RequestData, 8, out userIDEndIndex);

            if (ipAddress[0] == 0x0 && ipAddress[1] == 0x0 && ipAddress[2] == 0x0 && ipAddress[3] != 0x0) // Socks 4a
            {
                request.Version = "4a";

                int dummy;

                request.RemoteHost = SocksCommon.ReadNullTerminatedString(socks4RequestData, userIDEndIndex + 1, out dummy);

                if (tryToResolveHost)
                {
                    if (!IPAddress.TryParse(request.RemoteHost, out request.RemoteHostIP))
                    {
                        try { request.RemoteHostIP = Dns.GetHostEntry(request.RemoteHost).AddressList[0]; }
                        catch { return null; }
                    }
                }
            }
            else // Socks 4
            {
                request.Version = "4";
                try { request.RemoteHostIP = new IPAddress(ipAddress); }
                catch { return null; }
            }

            return request;
        }
    }
}
