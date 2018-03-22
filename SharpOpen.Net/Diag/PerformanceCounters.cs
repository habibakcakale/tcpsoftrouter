using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SharpOpen.Net
{
    internal static class PerformanceCounters
    {
        private static PerformanceCounter P1;
        private static PerformanceCounter P2;

        internal static void CreatePerformanceCounters()
        {
            // Create a collection of type CounterCreationDataCollection.
            CounterCreationDataCollection CounterDatas = new CounterCreationDataCollection();
            // Create the counters and set their properties.
            System.Diagnostics.CounterCreationData totalReceivedByteCounter = new System.Diagnostics.CounterCreationData();
            totalReceivedByteCounter.CounterName = "Bytes Received Total";
            totalReceivedByteCounter.CounterHelp = "The total number of bytes received from service clients since the last execution";
            totalReceivedByteCounter.CounterType = PerformanceCounterType.NumberOfItems64;

            System.Diagnostics.CounterCreationData perSecReceivedByteCounter = new System.Diagnostics.CounterCreationData();
            perSecReceivedByteCounter.CounterName = "Bytes Received Per Second";
            perSecReceivedByteCounter.CounterHelp = "Number of bytes received per second";
            perSecReceivedByteCounter.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;

            // Add both counters to the collection.
            CounterDatas.Add(totalReceivedByteCounter);
            CounterDatas.Add(perSecReceivedByteCounter);

            // Create the category and pass the collection to it.
            System.Diagnostics.PerformanceCounterCategory.Create
            (
                "SharpOpen Tcp Soft Router",
                "Performance Diagnostics for SharpOpen Tcp Soft Router Service",
                PerformanceCounterCategoryType.SingleInstance,
                CounterDatas
            );

            P1 = new PerformanceCounter("SharpOpen Tcp Soft Router", "Bytes Received Total", false);
            P2 = new PerformanceCounter("SharpOpen Tcp Soft Router", "Bytes Received Per Second", false);

            

        }

        internal static void BytesReceived(int numberOfBytes)
        {
            P1.IncrementBy(numberOfBytes);
        }


    }
}
