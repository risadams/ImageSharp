namespace ImageSharp.LoadTest.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    public class ExecutionStats
    {
        public ExecutionStats(IEnumerable<ServiceInvocationReport> reports, IEnumerable<MemoryLogEntry> memoryLog)
        {
            this.MemoryLog = memoryLog.ToArray();
            this.ReportsByTime = reports.OrderBy(r => r.StartTime).ToArray();
            this.ReportsByMegaPixels = this.ReportsByTime.OrderByDescending(r => r.MegaPixelsProcessed).ToArray();
            
            double totalMilliSeconds = 0;
            double totalMegaPixels = 0;
            foreach (ServiceInvocationReport r in this.ReportsByMegaPixels)
            {
                totalMilliSeconds += r.Milliseconds;
                totalMegaPixels += r.MegaPixelsProcessed;
            }

            this.TotalMegaPixels = totalMegaPixels;
            this.TotalMilliseconds = totalMilliSeconds;

            double totalBytes = GC.GetTotalMemory(false);
            this.TotalManagedMemoryInMegaBytes = totalBytes / (1024 * 1024);
            var process = Process.GetCurrentProcess();
            this.PeakWorkingSetMemoryInMegaBytes = process.PeakWorkingSet64 / (1024.0 * 1024.0);
            this.TotalUpTime = DateTime.Now - this.ReportsByTime[0].StartTime;
        }

        public double PeakWorkingSetMemoryInMegaBytes { get; }

        public double TotalManagedMemoryInMegaBytes { get; }

        public ServiceInvocationReport[] ReportsByMegaPixels { get; }

        public ServiceInvocationReport[] ReportsByTime { get; }

        public MemoryLogEntry[] MemoryLog { get; }

        public double TotalMilliseconds { get; }

        public double TotalMegaPixels { get; }

        public double AverageMillisecondsPerMegaPixel => this.TotalMilliseconds / this.TotalMegaPixels;

        public double AverageMillisecondsPerRequest => this.TotalMilliseconds / this.RequestCount;

        public int RequestCount => this.ReportsByMegaPixels.Length;

        public double AverageMegaPixels => this.TotalMegaPixels / this.RequestCount;

        public TimeSpan TotalUpTime { get; }

        public override string ToString()
        {
            var bld = new StringBuilder();
            bld.AppendLine($"[Total RequestCount: {this.RequestCount}] [Total up time: {this.TotalUpTime:mm\\:ss}]");
            bld.AppendLine(
                $"[Avg MP: {this.AverageMegaPixels:###.##}] [Max MP: {this.ReportsByMegaPixels[0].MegaPixelsProcessed:###.##}]");
            bld.AppendLine(
                $"[ms/MP: {this.AverageMillisecondsPerMegaPixel:###.}] [avg ms/req: {this.AverageMillisecondsPerRequest:###.}]");
            bld.AppendLine($"[Peak Working Set Memory: {this.PeakWorkingSetMemoryInMegaBytes:####.} MB] [GC: {this.TotalManagedMemoryInMegaBytes:####.} MB]");

            if (this.MemoryLog.Any())
            {
                bld.AppendLine("** Memory Log:");

                foreach (MemoryLogEntry e in this.MemoryLog)
                {
                    bld.AppendLine(e.ToString());
                }

                double avgWorkingSet = this.MemoryLog.Average(e => e.WorkingSetMegaBytes);
                bld.AppendLine($"[AVG Working set: {avgWorkingSet:####.} MB]");
            }
            
            return bld.ToString();
        }
    }
}