using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;


namespace SharpOpen.Net
{
    [RunInstaller(false)]
    public partial class PerformanceCounterInstallProvider : Installer
    {
        public PerformanceCounterInstallProvider()
        {
            InitializeComponent();

            PerformanceCounterInstaller performanceCounters = new PerformanceCounterInstaller();
            performanceCounters.CategoryName = "SharpOpen.Net";
            performanceCounters.CategoryHelp = "Performance counters for SharpOpen.Net";
            performanceCounters.CategoryType = PerformanceCounterCategoryType.SingleInstance;

            performanceCounters.Counters.Add(new CounterCreationData("Connections Accepted", "The number of connections accepted", PerformanceCounterType.RateOfCountsPerSecond32));
            performanceCounters.Counters.Add(new CounterCreationData("Bytes Sent", "Bytes sent to the remote end points", PerformanceCounterType.RateOfCountsPerSecond32));
            performanceCounters.Counters.Add(new CounterCreationData("Bytes Received", "Bytes received from remote end points", PerformanceCounterType.RateOfCountsPerSecond32));

            Installers.Add(performanceCounters);
        }
    }
}
