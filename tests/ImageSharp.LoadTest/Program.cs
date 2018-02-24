using System;

namespace ImageSharp.LoadTest
{
    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.Random;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Memory;

    class Program
    {
        public static void Main(string[] args)
        {
            Configuration.Default.MemoryManager = ArrayPoolMemoryManager.CreateWithConservativePooling();

            var randomSource = new Mrg32k3a(42, true);
            var megapixelDistribution = new Normal(4.0, 1.0, randomSource);
            var service = new ResizeService() { CleanOutput = true };

            var client =
                new TestClient(
                    service,
                    megapixelDistribution,
                    250,
                    randomSource) { AutoStopAfterNumberOfRequests = 500 };

            client.Run().Wait();

            Console.WriteLine("Stopped.");
            Console.ReadLine();
        }
    }
}
