namespace ImageSharp.LoadTest
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using MathNet.Numerics.Distributions;

    using SixLabors.ImageSharp.Tests;

    using Xunit.Sdk;

    public class InputProducer
    {
        private readonly IContinuousDistribution megaPixelDistribution;

        private readonly Dictionary<double, string> megapixel2Path = new Dictionary<double, string>();

        public InputProducer(IEnumerable<string> files, IContinuousDistribution megaPixelDistribution)
        {
            this.megaPixelDistribution = megaPixelDistribution;
            foreach (string path in files)
            {
                string fn = Path.GetFileNameWithoutExtension(path);
                string[] s = fn.Split('-');
                string mpString = s[s.Length - 2];

                double mp = double.Parse(mpString, CultureInfo.InvariantCulture);
                this.megapixel2Path[mp] = path;
            }
        }

        public int FileCount => this.megapixel2Path.Count;

        public static InputProducer Create(IContinuousDistribution distribution)
        {
            string solutionDir = TestEnvironment.SolutionDirectoryFullPath;
            string inputFileDir = $"{solutionDir}/tests/ImageSharp.LoadTest/GeneratedInput";
            var di = new DirectoryInfo(inputFileDir);
            IEnumerable<string> files = di.EnumerateFiles().Select(fi => fi.FullName);
            return new InputProducer(files, distribution);
        }

        public string GetFileByMegaPixels(double megaPixels)
        {
            double minDist = double.MaxValue;
            string bestMatch = null;

            foreach (KeyValuePair<double, string> kv in this.megapixel2Path)
            {
                double d = Math.Abs(kv.Key - megaPixels);
                if (d < minDist)
                {
                    minDist = d;
                    bestMatch = kv.Value;
                }
            }

            return bestMatch;
        }

        public string Next()
        {
            double mp = this.megaPixelDistribution.Sample();
            return this.GetFileByMegaPixels(mp);
        }
    }
}