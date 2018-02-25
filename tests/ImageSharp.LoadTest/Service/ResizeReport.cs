namespace ImageSharp.LoadTest.Service
{
    using System;
    using System.IO;

    using ImageSharp.LoadTest.Statistics;

    using SixLabors.Primitives;

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