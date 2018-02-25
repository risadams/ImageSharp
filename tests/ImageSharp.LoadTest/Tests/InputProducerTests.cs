// ReSharper disable InconsistentNaming
namespace ImageSharp.LoadTest.Tests
{
    using System.IO;

    using ImageSharp.LoadTest.Execution;

    using MathNet.Numerics.Distributions;
    using Xunit;
    using Xunit.Abstractions;

    public class InputProducerTests
    {
        public static string[] PathValues =
            {
                "MP-1.000-foo.jpg",
                "MP-0.990-foo.jpg",
                "MP-0.943-Trash.jpg",
                "MP-1.112-Baz.jpg",
                "MP-1.1-foo.jpg",
            };

        public static readonly TheoryData<double, string> GetFileByMegaPixels_Data = new TheoryData<double, string>()
                                                                                    {
                                                                                        { 1.0, PathValues[0] },
                                                                                        { 0.996, PathValues[0] },
                                                                                        { 0.99, PathValues[1] },
                                                                                        { 0.9, PathValues[2] },
                                                                                        { 1.3, PathValues[3] },
                                                                                        { 1.1, PathValues[4] },
                                                                                    };

        private readonly IContinuousDistribution dummyDistribution = new Normal();

        public InputProducerTests(ITestOutputHelper output)
        {
            this.Output = output;
        }

        private ITestOutputHelper Output { get; }

        [Theory]
        [MemberData(nameof(GetFileByMegaPixels_Data))]
        public void GetFileByMegaPixels(double mp, string expectedPath)
        {
            var producer = new InputProducer(PathValues, () => 42.0);

            string actualPath = producer.GetFileByMegaPixels(mp);

            Assert.Equal(expectedPath, actualPath);
        }

        [Fact]
        public void Create()
        {
            var producer = InputProducer.Create(this.dummyDistribution);

            Assert.True(producer.FileCount > 0);
            string path = producer.GetFileByMegaPixels(3.1);
            this.Output.WriteLine(path);
            Assert.True(File.Exists(path));
        }

        [Fact]
        public void Next()
        {
            var distribution = new Normal(3.0, 1.0);
            var producer = InputProducer.Create(distribution);

            for (int i = 0; i < 10; i++)
            {
                string path = producer.Next();
                this.Output.WriteLine(path);
            }
        }
    }
}