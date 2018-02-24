#pragma warning disable 4014
namespace ImageSharp.LoadTest
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using MathNet.Numerics.Distributions;

    using SixLabors.ImageSharp;

    public class Stats
    {
        public Stats(IEnumerable<ServiceInvocationReport> reports)
        {
            this.ReportsByTime = reports.OrderBy(r => r.StartTime).ToArray();
            this.ReportsByMegaPixels = this.ReportsByTime.OrderByDescending(r => r.MegaPixelsProcessed).ToArray();
            this.ReportsByMegaPixels = reports.ToArray();
            
            double totalMilliSeconds = 0;
            double totalMegaPixels = 0;
            foreach (ServiceInvocationReport r in this.ReportsByMegaPixels)
            {
                totalMilliSeconds += r.Milliseconds;
                totalMegaPixels += r.MegaPixelsProcessed;
            }

            this.TotalMegaPixels = totalMegaPixels;
            this.TotalMilliseconds = totalMilliSeconds;
        }

        public ServiceInvocationReport[] ReportsByMegaPixels { get; }

        public ServiceInvocationReport[] ReportsByTime { get; }

        public double TotalMilliseconds { get; }

        public double TotalMegaPixels { get; }

        public double AverageMillisecondsPerMegaPixel => this.TotalMilliseconds / this.TotalMegaPixels;

        public int TotalRequests => this.ReportsByMegaPixels.Length;

        public double AverageMegaPixels => this.TotalMegaPixels / this.TotalRequests;

        public override string ToString()
        {
            return
                $"[Avg MP: {this.AverageMegaPixels}] [Max MP: {this.ReportsByMegaPixels[0].MegaPixelsProcessed}] [ms/MP: {this.AverageMillisecondsPerMegaPixel}]";
        }
    }

    public class TestClient
    {
        private InputProducer inputProducer;

        private ITestService service;

        private readonly IContinuousDistribution requestDensityMillisecondsDistribution;

        private int requestsSent = 0;

        private readonly ConcurrentBag<ServiceInvocationReport> processed = new ConcurrentBag<ServiceInvocationReport>();

        public TestClient(
            ITestService service,
            IContinuousDistribution megapixelDistribution,
            IContinuousDistribution requestDensityMillisecondsDistribution
            )
        {
            this.inputProducer = InputProducer.Create(megapixelDistribution);
            this.service = service;
            this.requestDensityMillisecondsDistribution = requestDensityMillisecondsDistribution;
        }

        public TestClient(
            ITestService service,
            IContinuousDistribution megapixelDistribution,
            double averageMsBetweenRequests,
            Random randomSource)
            : this(service, megapixelDistribution, new Exponential(1.0 / averageMsBetweenRequests, randomSource))
        {
        }

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
                if (this.ProcessConsole())
                {
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

                double waitMs = this.requestDensityMillisecondsDistribution.Sample();
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
                        Configuration.Default.MemoryManager.ReleaseRetainedResources();
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