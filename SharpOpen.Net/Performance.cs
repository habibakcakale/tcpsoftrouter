using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SharpOpen.Net
{
    internal static class Performance
    {
        internal static PerformanceCounter ConnectionsAccepted;
        internal static PerformanceCounter BytesSent;
        internal static PerformanceCounter BytesReceived;

        internal static void UpdateConnectoinAccepted()
        {
            if (ConnectionsAccepted != null) ConnectionsAccepted.Increment();
        }
        internal static void UpdateBytesReceived(int count)
        {
            if (BytesReceived  != null) BytesReceived.IncrementBy(count);
        }
        internal static void UpdateBytesSent(int count)
        {
            if (BytesSent != null) BytesSent.IncrementBy(count);
        }

        static Performance()
        {
            try
            {
                if (PerformanceCounterCategory.Exists("SharpOpen.Net"))
                {
                    ConnectionsAccepted = new PerformanceCounter("SharpOpen.Net", "Connections Accepted", false);
                    BytesSent = new PerformanceCounter("SharpOpen.Net", "Bytes Sent", false);
                    BytesReceived = new PerformanceCounter("SharpOpen.Net", "Bytes Received", false);
                }
            }
            catch { }
        }
    }
}
