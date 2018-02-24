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
            Configuration.Default.MemoryManager = ArrayPoolMemoryManager.CreateWithAggressivePooling();

            var randomSource = new Mrg32k3a(true);
            var megapixelDistribution = new Normal(3.0, 1.0, randomSource);
            var service = new ResizeService() { CleanOutput = true };

            var client = new TestClient(service, megapixelDistribution, 500, randomSource);

            client.Run().Wait();

            Console.WriteLine("Stopped.");
            Console.ReadLine();
        }
    }
}
