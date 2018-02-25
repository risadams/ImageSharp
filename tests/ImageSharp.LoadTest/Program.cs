using System;

namespace ImageSharp.LoadTest
{
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.Random;

    using SixLabors.ImageSharp;
#if !RELEASE_OLD
    using SixLabors.ImageSharp.Memory;
#endif

    class Program
    {
        public static bool Verbose { get; set; } = true;

        public static void Main(string[] args)
        {
            InitMemoryManager(args);

            int meanImageWidth = 3200;
            int imageWidthDeviation = 1000;
            int averageMsBetweenRequests = 500;

            if (args.Length > 3)
            {
                meanImageWidth = int.Parse(args[1], CultureInfo.InvariantCulture);
                imageWidthDeviation = int.Parse(args[2], CultureInfo.InvariantCulture);
                averageMsBetweenRequests = int.Parse(args[3], CultureInfo.InvariantCulture);
            }

            Verbose = args.Length == 0 || args.Length > 4 && args[4].ToLower().StartsWith("v");

            Console.WriteLine(
                $"meanImageWidth={meanImageWidth} | imageWidthDeviation={imageWidthDeviation} | averageMsBetweenRequests={averageMsBetweenRequests}");
            
            var service = new ResizeService() { CleanOutput = true };

            var client = TestClient.CreateWithLogNormalLoad(
                service,
                meanImageWidth,
                imageWidthDeviation,
                averageMsBetweenRequests);

            client.AutoStopAfterNumberOfRequests = 610;
            client.LogMemoryEachRequest = 50;

            client.Run().Wait();

            if (Program.Verbose)
            {
                Console.WriteLine("Stopped.");
                Console.ReadLine();
            }
        }

        private static void InitMemoryManager(string[] args)
        {
#if !RELEASE_OLD
            MemoryManager memoryManager = null;
            if (args.Length > 0)
            {
                string poolingMode = args[0].ToLower();
                if (poolingMode == "nopooling")
                {
                    Console.WriteLine("MemoryManager: SimpleManagedMemoryManager");
                    memoryManager = new SimpleManagedMemoryManager();
                }
                else
                {
                    MethodInfo factoryMethod = typeof(ArrayPoolMemoryManager).GetTypeInfo()
                        .GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name.ToLower() == poolingMode);

                    if (factoryMethod != null)
                    {
                        Console.WriteLine("MemoryManager: ArrayPoolMemoryManager." + args[0]);
                        memoryManager = (ArrayPoolMemoryManager)factoryMethod.Invoke(null, new object[0]);
                    }
                }
            }
            else
            {
                memoryManager = ArrayPoolMemoryManager.CreateWithNormalPooling2();
            }

            Configuration.Default.MemoryManager = memoryManager;
#endif
        }
    }
}
