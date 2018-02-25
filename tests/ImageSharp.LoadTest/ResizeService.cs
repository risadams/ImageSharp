namespace ImageSharp.LoadTest
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    using SixLabors.ImageSharp;
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

            if (Program.Verbose)
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

    public class ResizeReport : ServiceInvocationReport
    {
        private readonly Size originalSize;

        public ResizeReport(DateTime startTime, string inputFile, string resultFile, Size originalSize, long milliseconds)
            : base(startTime, originalSize.GetMegaPixels(), milliseconds)
        {
            this.originalSize = originalSize;
            this.InputFile = inputFile;
            this.ResultFile = resultFile;
        }

        public string InputFile { get; }

        public string ResultFile { get; }

        public override string ToString()
        {
            string fn = Path.GetFileNameWithoutExtension(this.InputFile);
            return $"[Resized: {fn} ({this.originalSize.Width}x{this.originalSize.Height}, {this.MegaPixelsProcessed:0.00}MP) in {this.Milliseconds:0000}ms]";
        }
    }
}