using System;

namespace ImageSharp.LoadTest
{
    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.Random;

    using SixLabors.ImageSharp;
#if !RELEASE_OLD
    using SixLabors.ImageSharp.Memory;
#endif

    class Program
    {
        public static void Main(string[] args)
        {
#if !RELEASE_OLD
            Configuration.Default.MemoryManager = ArrayPoolMemoryManager.CreateWithNormalPooling();
#endif

            int meanImageWidth = 4000;
            int imageWidthDeviation = 1500;
            int averageMsBetweenRequests = 400;
            
            var service = new ResizeService() { CleanOutput = true };

            var client = TestClient.CreateWithLogNormalLoad(
                service,
                meanImageWidth,
                imageWidthDeviation,
                averageMsBetweenRequests);
            client.AutoStopAfterNumberOfRequests = 1000;

            client.Run().Wait();

            Console.WriteLine("Stopped.");
            Console.ReadLine();
        }
    }
}
