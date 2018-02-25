// ReSharper disable InconsistentNaming
namespace ImageSharp.LoadTest.Statistics
{
    using System;
    using System.Diagnostics;

    public struct MemoryLogEntry
    {
        public MemoryLogEntry(
            int requestCount,
            TimeSpan elapsedTime,
            double privateMegaBytes,
            double workingSetMegaBytes,
            double virtualMegaBytes,
            double gcMegaBytes)
        {
            this.RequestCount = requestCount;
            this.ElapsedTime = elapsedTime;

            this.PrivateMegaBytes = privateMegaBytes;
            this.WorkingSetMegaBytes = workingSetMegaBytes;
            this.VirtualMegaBytes = virtualMegaBytes;
            this.GCMegaBytes = gcMegaBytes;
        }

        public static MemoryLogEntry Create(DateTime startTime, int requestCount)
        {
            var process = Process.GetCurrentProcess();
            const double BytesInMb = (1024.0 * 1024.0);

            double privateMb = process.PrivateMemorySize64 / BytesInMb;
            double workingSetMb = process.WorkingSet64 / BytesInMb;
            double virtMb = process.VirtualMemorySize64 / BytesInMb;
            TimeSpan dt = DateTime.Now - startTime;

            double gcMb = GC.GetTotalMemory(true) / BytesInMb;

            return new MemoryLogEntry(requestCount, dt, privateMb, workingSetMb, virtMb, gcMb);
        }

        public int RequestCount { get; }

        public double PrivateMegaBytes { get; }

        public double WorkingSetMegaBytes { get; }

        public double VirtualMegaBytes { get; }

        public double GCMegaBytes { get; }

        public TimeSpan ElapsedTime { get; }

        public override string ToString()
        {
            return $"{this.ElapsedTime:mm\\:ss} | WorkingSet: {this.WorkingSetMegaBytes,5:#####.} MB | GC: {this.GCMegaBytes,4:#####.} MB";
        }
    }
}