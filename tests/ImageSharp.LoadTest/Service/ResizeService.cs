namespace ImageSharp.LoadTest.Service
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    using ImageSharp.LoadTest.Statistics;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.Processing.Transforms;
    using SixLabors.Primitives;

    public class ResizeService : ITestService
    {
        private const string OutputDir = ".Output";

        public bool CleanOutput { get; set; }

        public Task<ServiceInvocationReport> ProcessImage(Func<string> pathProducer)
        {
            string path = pathProducer();
            return Task<ServiceInvocationReport>.Factory.StartNew(
                () => this.ProcessImpl(path),
                TaskCreationOptions.LongRunning);
        }

        public ResizeService()
        {
            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }
        }

        private static string CreateDummyOutputFile()
        {
            string fn = $"{OutputDir}/{Guid.NewGuid().ToString().Substring(0, 6)}.jpeg";
            return Path.GetFullPath(fn);
        }

        private ServiceInvocationReport ProcessImpl(string path)
        {
            DateTime time = DateTime.Now;

            if (Program.IsInteractive)
            {
                Console.WriteLine("  Resize Request: " + path);
            }
            
            var stopwatch = Stopwatch.StartNew();
            
            using (var image = SixLabors.ImageSharp.Image.Load(path))
            {
                var origialSize = new Size(image.Width, image.Height);
                int w = image.Width / 4;
                int h = image.Height / 4;

                image.Mutate(c => c.Resize(w, h));

                string resultFile = CreateDummyOutputFile();
                image.Save(resultFile);
                stopwatch.Stop();
                if (this.CleanOutput)
                {
                    File.Delete(resultFile);
                }

                return new ResizeReport(time, path, resultFile, origialSize, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}