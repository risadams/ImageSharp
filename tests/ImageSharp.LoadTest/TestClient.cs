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
        public Stats(IEnumerable<ServiceInvocationReport> reports)
        {
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
            this.TotalMemoryInMegaBytes = Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0);
            this.TotalUpTime = DateTime.Now - this.ReportsByTime[0].StartTime;
        }

        public double TotalMemoryInMegaBytes { get; }

        public double TotalManagedMemoryInMegaBytes { get; }

        public ServiceInvocationReport[] ReportsByMegaPixels { get; }

        public ServiceInvocationReport[] ReportsByTime { get; }

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
            bld.AppendLine($"[Total RequestCount: {this.RequestCount}] [Total up time: {this.TotalUpTime}]");
            bld.AppendLine(
                $"[Avg MP: {this.AverageMegaPixels}] [Max MP: {this.ReportsByMegaPixels[0].MegaPixelsProcessed}]");
            bld.AppendLine(
                $"[ms/MP: {this.AverageMillisecondsPerMegaPixel:##.}] [avg ms/req: {this.AverageMillisecondsPerRequest:##.}]");
            bld.AppendLine($"[Memory: {this.TotalMemoryInMegaBytes:##.} MB] [GC: {this.TotalManagedMemoryInMegaBytes:##.} MB]");

            return bld.ToString();
        }
    }

    public class TestClient
    {
        private InputProducer inputProducer;

        private ITestService service;

        private readonly Func<double> requestDensityMillisecondsSampler;

        private int requestsSent = 0;

        private readonly ConcurrentBag<ServiceInvocationReport> processed = new ConcurrentBag<ServiceInvocationReport>();

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

        private void PrintStats()
        {
            Console.WriteLine("**** Stats ******");
            var stats = new Stats(this.processed);
            Console.WriteLine(stats.ToString());
            Console.WriteLine("*****************");
        }
        
        public async Task Run()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("   ESC: Stop");
            Console.WriteLine("   R:   ReleaseRetainedResources()");
            Console.WriteLine("   S:   Stats");
            Console.WriteLine("\nPress enter to start!\n***********");
            Console.ReadLine();

            for (;;)
            {
                if (this.ProcessConsole() || this.processed.Count > this.AutoStopAfterNumberOfRequests)
                {
                    this.PrintStats();
                    return;
                }
                
                Task<ServiceInvocationReport> process = this.service.ProcessImage(this.inputProducer.Next);
                Interlocked.Increment(ref this.requestsSent);
                process.ContinueWith(
                    _ =>
                        {
                            ServiceInvocationReport result = process.Result;
                            Interlocked.Decrement(ref this.requestsSent);
                            this.processed.Add(result);
                            Console.WriteLine($"Finished: {result}");
                            Console.WriteLine($"  Total: {this.processed.Count} | Requests in queue: {this.requestsSent}");
                            Console.Out.Flush();
                        });

                double waitMs = this.requestDensityMillisecondsSampler();
                await Task.Delay((int)waitMs);
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