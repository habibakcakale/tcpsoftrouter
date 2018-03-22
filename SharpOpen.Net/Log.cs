using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SharpOpen.Net
{
    public static class Log
    {
        public static string ByteArrayToHexString(byte[] rawData)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rawData.Length; i++) sb.AppendFormat("{0}", rawData[i].ToString("X").PadLeft(2, '0'));
            return sb.ToString();
        }

        public static byte[] HexStringToByteArray(string hexString)
        {
            MemoryStream stream = new MemoryStream(hexString.Length / 2);

            for (int i = 0; i < hexString.Length; i += 2)
            {
                stream.WriteByte(byte.Parse(hexString.Substring(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            return stream.ToArray();
        }

        public static void Write(string message)
        {
            Trace.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss} {1:00000} --> {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, message));
        }

        public static void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }

    }
}
