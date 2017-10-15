using JAR.Client.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Timers;

namespace JAR.Client
{
    internal class Ticker
    {
        internal void Pause()
        {
            tmrTicker.Enabled = false;
        }

        internal void Start()
        {
            tmrTicker.Enabled = true;
        }




        private PerformanceCounter cpuCounter;
        //private PerformanceCounter memCounter;
        //private List<PerformanceCounter> cpuCounters = new List<PerformanceCounter>();
        private Timer tmrTicker;
        private ManagementObjectSearcher wmiObject;

        public Ticker()
        {
            tmrTicker = new Timer(1000);
            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            //memCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            //int procCount = Environment.ProcessorCount;
            //for (int i = 0; i < procCount; i++)
            //{
            //    PerformanceCounter pc = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
            //    cpuCounters.Add(pc);
            //}
            wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            tmrTicker.Elapsed += TmrTicker_Elapsed;
        }

        private void TmrTicker_Elapsed(object sender, ElapsedEventArgs e)
        {
            float cpu = cpuCounter.NextValue();
            //float avgcores = 0;
            //List<float> cores = new List<float>();
            //foreach (PerformanceCounter c in cpuCounters)
            //{
            //    var core = c.NextValue();
            //    avgcores = avgcores + core;
            //    cores.Add(core);
            //}
            //avgcores = avgcores / (Environment.ProcessorCount);
            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = float.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = float.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();
            float mem = 0;
            if (memoryValues != null)
            {
                mem = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
            }
            OnTick?.Invoke(this, new TickArgs(cpu, mem));//, avgcores, cores));
            Console.WriteLine(string.Format("CPU: {0}, MemUsage: {1}", cpu, mem));
        }
        #region EventsArgs
        public class TickArgs : EventArgs
        {
            public float CPUTotal { get; set; }
            public float MemoryUsed { get; set; }
            //public float AverageCores { get; set; }
            //public List<float> Cores { get; set; }
            public TickMessage TickMsg { get; set; }

            public TickArgs(float cputotal, float mem)//, float avgcores, List<float> cores)
            {
                CPUTotal = cputotal;
                MemoryUsed = mem;
                //AverageCores = avgcores;
                //Cores = cores;
                TickMsg = new TickMessage(
                    CPUTotal: cputotal,
                    MemoryUsed: mem
                //AverageCores: avgcores
                //Cores: cores.ToArray()
                );
            }
        }
        #endregion

        #region Ticker Events
        public event EventHandler<TickArgs> OnTick;
        #endregion
    }
}