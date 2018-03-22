using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SharpOpen.Net.Socks
{
    /*
    
        field 1: null byte 
        field 2: status, 1 byte: 
            0x5a = request granted 
            0x5b = request rejected or failed 
            0x5c = request failed because client is not running identd (or not reachable from the server) 
            0x5d = request failed because client's identd could not confirm the user ID string in the request 
        field 3: network byte order port number, 2 bytes 
        field 4: network byte order IP address, 4 bytes     
    
    */

    public class Socks4Response
    {
        public byte Status;
        public int Port;
        public string ResolvedIP;
        
        public bool Success { get { return Status == 0x5A; } }

        public static readonly byte[] REJECT_RESPONSE = new byte[] { 0x00, 0x5B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
 
        private Socks4Response() { }

        public static Socks4Response Parse(byte[] socks4ResponseData)
        {
            if (socks4ResponseData == null) return null;
            if (socks4ResponseData.Length < 8) return null;
            if (socks4ResponseData[0] != 0x00) return null;

            Socks4Response response = new Socks4Response();
            response.Status = socks4ResponseData[1];

            response.Port = (socks4ResponseData[2] << 8) + socks4ResponseData[3];

            response.ResolvedIP = string.Format("{0}.{1}.{2}.{3}", socks4ResponseData[4], socks4ResponseData[5], socks4ResponseData[6], socks4ResponseData[7]);

            return response;
        }
    }
}
