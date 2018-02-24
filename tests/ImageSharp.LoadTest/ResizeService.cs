namespace ImageSharp.LoadTest
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Helpers;
    using SixLabors.Primitives;

    public class ResizeService : ITestService
    {
        private const string OutputDir = ".Output";

        public bool CleanOutput { get; set; }

        public Task<ServiceInvocationReport> ProcessImage(Func<string> pathProducer)
        {
            return Task<ServiceInvocationReport>.Factory.StartNew(
                () => this.ProcessImpl(pathProducer()),
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
            Console.WriteLine("  Resize Request: " + path);
            Console.Out.Flush();

            var stopwatch = Stopwatch.StartNew();
            using (var image = Image.Load(path))
            {
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

                return new ResizeReport(time, path, resultFile, image, stopwatch.ElapsedMilliseconds);
            }
        }
    }

    public class ResizeReport : ServiceInvocationReport
    {
        private readonly Size size;

        public ResizeReport(DateTime startTime, string inputFile, string resultFile, IImage image, long milliseconds)
            : base(startTime, image.GetMegaPixels(), milliseconds)
        {
            this.size = new Size(image.Width, image.Height);
            this.InputFile = inputFile;
            this.ResultFile = resultFile;
        }

        public string InputFile { get; }

        public string ResultFile { get; }

        public override string ToString()
        {
            string fn = Path.GetFileNameWithoutExtension(this.InputFile);
            return $"[Resized: {fn} ({this.size.Width}x{this.size.Height}, {this.MegaPixelsProcessed:0.00}MP) in {this.Milliseconds:0000}ms]";
        }
    }
}