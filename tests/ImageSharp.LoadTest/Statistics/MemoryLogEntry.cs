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
            double virtualMegaBytes)
        {
            this.RequestCount = requestCount;
            this.ElapsedTime = elapsedTime;

            this.PrivateMegaBytes = privateMegaBytes;
            this.WorkingSetMegaBytes = workingSetMegaBytes;
            this.VirtualMegaBytes = virtualMegaBytes;
        }

        public static MemoryLogEntry Create(DateTime startTime, int requestCount)
        {
            var process = Process.GetCurrentProcess();
            const double BytesInMb = (1024.0 * 1024.0);

            double privateMb = process.PrivateMemorySize64 / BytesInMb;
            double workingSetMb = process.WorkingSet64 / BytesInMb;
            double virtMb = process.VirtualMemorySize64 / BytesInMb;
            TimeSpan dt = DateTime.Now - startTime;
            return new MemoryLogEntry(requestCount, dt, privateMb, workingSetMb, virtMb);
        }

        public int RequestCount { get; }

        public double PrivateMegaBytes { get; }

        public double WorkingSetMegaBytes { get; }

        public double VirtualMegaBytes { get; }

        public TimeSpan ElapsedTime { get; }

        public override string ToString()
        {
            return $"{this.ElapsedTime:mm\\:ss} | WorkingSet: {this.WorkingSetMegaBytes:000.0} MB";
        }
    }
}