using System;

namespace ImageSharp.LoadTest
{
    using System.Linq;
    using System.Reflection;

    using ImageSharp.LoadTest.Execution;
    using ImageSharp.LoadTest.Service;

    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.Random;

    using SixLabors.ImageSharp;
#if !RELEASE_OLD
    using SixLabors.ImageSharp.Memory;
#endif

    class Program
    {
        public static bool IsInteractive { get; set; } = true;
        
        public static void Main(string[] args)
        {
            InitMemoryManager(args);

            var p = ExecutionParameters.Parse(args);
            IsInteractive = p.IsInteractive;

            Console.WriteLine(p.ToString());
            
            var service = new ResizeService() { CleanOutput = true };

            var client = TestClient.CreateWithLogNormalLoad(
                service,
                p.MeanImageWidth,
                p.ImageWidthDeviation,
                p.AverageMsBetweenRequests);

            client.AutoStopAfterNumberOfRequests = p.NoOfReq;
            client.LogMemoryEachRequest = 50;

            client.Run().Wait();

            if (Program.IsInteractive)
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
                memoryManager = ArrayPoolMemoryManager.CreateDefault();
            }

            Configuration.Default.MemoryManager = memoryManager;
#endif
        }
    }
}
