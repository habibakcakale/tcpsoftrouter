using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SharpOpen.Net.Socks
{
    public static class SocksCommon
    {
        public static void SendSocksError(Connection client)
        {
            client.Send(Socks4Response.REJECT_RESPONSE);
            client.Close();
        }

        public static void SendSocksSuccess(Connection client, int port, IPAddress remoteHostIP)
        {
            byte[] ipAddressbytes = new byte[4];
            string[] ipParts = remoteHostIP.ToString().Split('.');

            for (int i = 0; i < 4; i++) ipAddressbytes[i] = byte.Parse(ipParts[i]);

            client.Send(new byte[] { 0x00, 0x5A, (byte)(port >> 8), (byte)(port & 0xFF), ipAddressbytes[0], ipAddressbytes[1], ipAddressbytes[2], ipAddressbytes[3] });
        }

        public static string ReadNullTerminatedString(byte[] array, int startIndex, out int endIndex)
        {
            endIndex = startIndex;
            for (int i = startIndex; i < array.Length; i++)
            {
                if (array[i] == 0x0)
                {
                    endIndex = i;
                    return Encoding.ASCII.GetString(array, startIndex, i - startIndex);
                }
            }
            return string.Empty;
        }


    }
}
