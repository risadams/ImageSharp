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
            Configuration.Default.MemoryManager = ArrayPoolMemoryManager.CreateWithNormalPooling();

            int meanImageWidth = 4000;
            int imageWidthDeviation = 2500;
            int averageMsBetweenRequests = 500;
            
            var service = new ResizeService() { CleanOutput = true };

            var client = TestClient.CreateWithLogNormalLoad(
                service,
                meanImageWidth,
                imageWidthDeviation,
                averageMsBetweenRequests);
            client.AutoStopAfterNumberOfRequests = 500;

            client.Run().Wait();

            Console.WriteLine("Stopped.");
            Console.ReadLine();
        }
    }
}
