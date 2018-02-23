#pragma warning disable 4014
namespace ImageSharp.LoadTest
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using MathNet.Numerics.Distributions;

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
        
        public async Task Run()
        {
            Console.WriteLine("Press escape to stop execution!");

            for (;;)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
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
    }
}