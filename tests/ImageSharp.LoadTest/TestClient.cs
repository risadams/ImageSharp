#pragma warning disable 4014
namespace ImageSharp.LoadTest
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.Random;

    using SixLabors.ImageSharp;

    public class Stats
    {
        public Stats(IEnumerable<ServiceInvocationReport> reports, IEnumerable<MemoryLogEntry> memoryLog)
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

        public double PeakPrivateMemoryInMegaBytes { get; }

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

    public class TestClient
    {
        private InputProducer inputProducer;

        private ITestService service;

        private readonly Func<double> requestDensityMillisecondsSampler;

        private int requestsSent = 0;

        private readonly ConcurrentBag<ServiceInvocationReport> processed = new ConcurrentBag<ServiceInvocationReport>();

        private readonly List<MemoryLogEntry> memoryLog = new List<MemoryLogEntry>();

        private int requestsAfterLastMemoryLog = 0;

        public TestClient(
            ITestService service,
            Func<double> megaPixelDistributionSampler,
            Func<double> requestDensityMillisecondsSampler
            )
        {
            this.inputProducer = InputProducer.Create(megaPixelDistributionSampler);
            this.service = service;
            this.requestDensityMillisecondsSampler = requestDensityMillisecondsSampler;
        }
        
        public TestClient(
            ITestService service,
            Func<double> megaPixelDistributionSampler,
            int averageMsBetweenRequests,
            Random randomSource)
            : this(service, megaPixelDistributionSampler, new Exponential(1.0 / averageMsBetweenRequests, randomSource).Sample)
        {
        }
        
        public static TestClient CreateWithLogNormalLoad(
            ITestService service,
            int meanImageWidth,
            int imageWidthDeviation,
            int averageMsBetweenRequests)
        {
            var randomSource = new Mrg32k3a(42, true);
            double variance = imageWidthDeviation * imageWidthDeviation;
            var widthDistribution = LogNormal.WithMeanVariance(meanImageWidth, variance, randomSource);

            double MegaPixelSampler()
            {
                double width = widthDistribution.Sample();
                
                double height = width / 1.77; // typical aspect ratio
                return width * height / (1_000_000);
            }

            return new TestClient(service, MegaPixelSampler, averageMsBetweenRequests, randomSource);
        }

        public static TestClient CreateClientWithNormalLoad(ITestService service,
                                                            int meanImageWidth,
                                                            int imageWidthDeviation,
                                                            int averageMsBetweenRequests)
        {
            var randomSource = new Mrg32k3a(42, true);
            var widthDistribution = new Normal(meanImageWidth, imageWidthDeviation, randomSource);

            double MegaPixelSampler()
            {
                double width = widthDistribution.Sample();

                double height = width / 1.77; // typical aspect ratio
                return width * height / (1_000_000);
            }

            return new TestClient(service, MegaPixelSampler, averageMsBetweenRequests, randomSource);
        }

        public int AutoStopAfterNumberOfRequests { get; set; } = int.MaxValue;

        public int LogMemoryEachRequest { get; set; } = 100;

        private void PrintStats()
        {
            Console.WriteLine("**** Stats ******");
            var stats = new Stats(this.processed, this.memoryLog);
            Console.WriteLine(stats.ToString());
            Console.WriteLine("*****************");
        }

        
        public async Task Run()
        {
            if (Program.Verbose)
            {
                Console.WriteLine("Commands:");
                Console.WriteLine("   ESC: Stop");
                Console.WriteLine("   R:   ReleaseRetainedResources()");
                Console.WriteLine("   S:   Stats");
                Console.WriteLine("\nPress enter to start!\n***********");
                Console.ReadLine();
            }
            
            DateTime startTime = DateTime.Now;

            for (;;)
            {
                if (this.ProcessConsole() || this.processed.Count > this.AutoStopAfterNumberOfRequests)
                {
                    this.PrintStats();
                    return;
                }

                this.LogMemoryUsage(startTime);
                
                Task<ServiceInvocationReport> process = this.service.ProcessImage(this.inputProducer.Next);
                Interlocked.Increment(ref this.requestsSent);
                process.ContinueWith(
                    _ =>
                        {
                            ServiceInvocationReport result = process.Result;
                            Interlocked.Decrement(ref this.requestsSent);
                            Interlocked.Increment(ref this.requestsAfterLastMemoryLog);
                            this.processed.Add(result);

                            if (Program.Verbose)
                            {
                                Console.WriteLine($"Finished: {result}");
                                Console.WriteLine($"  Total: {this.processed.Count} | Requests in queue: {this.requestsSent}");
                                Console.Out.Flush();
                            }
                        });

                double waitMs = this.requestDensityMillisecondsSampler();
                await Task.Delay((int)waitMs);
            }
        }

        private void LogMemoryUsage(DateTime startTime)
        {
            if (this.requestsAfterLastMemoryLog > this.LogMemoryEachRequest)
            {
                var entry = MemoryLogEntry.Create(startTime, this.processed.Count);
                this.memoryLog.Add(entry);
                int r = this.requestsAfterLastMemoryLog - this.LogMemoryEachRequest;
                Interlocked.Exchange(ref this.requestsAfterLastMemoryLog, r);
            }
        }

        private bool ProcessConsole()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        return true;
                    case ConsoleKey.R:
#if !RELEASE_OLD
                        Configuration.Default.MemoryManager.ReleaseRetainedResources();
#endif
                        Console.WriteLine(
                            "******** Configuration.Default.MemoryManager.ReleaseRetainedResources() called! ********");
                        break;
                    case ConsoleKey.G:
                        GC.Collect();
                        Console.WriteLine("******** GC.Collect() called! ***************");
                        break;
                    case ConsoleKey.S:
                        this.PrintStats();
                        break;
                }
            }

            return false;
        }
    }
}