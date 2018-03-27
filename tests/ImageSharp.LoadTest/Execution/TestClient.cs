#pragma warning disable 4014
namespace ImageSharp.LoadTest.Execution
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;
    using System.Threading.Tasks;

    using ImageSharp.LoadTest.Service;
    using ImageSharp.LoadTest.Statistics;

    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.Random;

    using SixLabors.ImageSharp;

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
            var stats = new ExecutionStats(this.processed, this.memoryLog);
            Console.WriteLine(stats.ToString());
            Console.WriteLine("*****************");
        }

        
        public async Task Run()
        {
            if (Program.IsInteractive)
            {
                Console.WriteLine("Commands:");
                Console.WriteLine("   ESC: Stop");
                Console.WriteLine("   R:   ReleaseRetainedResources()");
                Console.WriteLine("   S:   Stats");
                Console.WriteLine("   D:   switch to dummy mode, keep benchmarking");
                Console.WriteLine("   X:   ReleaseRetainedResources() + GC + switch to dummy mode, keep benchmarking");
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

                            if (Program.IsInteractive)
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
                    case ConsoleKey.D:
                        this.service = new DummyService();
                        break;
                    case ConsoleKey.X:
                        Console.WriteLine(" ~~~~~~ ReleaseRetainedResources() + GC + switch to dummy mode, keep benchmarking ~~~~~~~~~~~");
                        this.service = new DummyService();
#if !RELEASE_OLD
                        Configuration.Default.MemoryManager.ReleaseRetainedResources();
#endif
                        Thread.Sleep(10);
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect();
                        break;
                }
            }

            return false;
        }
    }
}