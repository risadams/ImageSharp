namespace ImageSharp.LoadTest.Tests
{
    using System.IO;

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

        public static readonly TheoryData<double, string> MegaPixel2Path_Data = new TheoryData<double, string>()
                                                                                    {
                                                                                        { 1.0, PathValues[0] },
                                                                                        { 0.996, PathValues[0] },
                                                                                        { 0.99, PathValues[1] },
                                                                                        { 0.9, PathValues[2] },
                                                                                        { 1.3, PathValues[3] },
                                                                                        { 1.1, PathValues[4] },
                                                                                    };

        public InputProducerTests(ITestOutputHelper output)
        {
            this.Output = output;
        }

        private ITestOutputHelper Output { get; }

        [Theory]
        [MemberData(nameof(MegaPixel2Path_Data))]
        public void MegaPixel2Path(double mp, string expectedPath)
        {
            var producer = new InputProducer(PathValues);

            string actualPath = producer.GetPath(mp);

            Assert.Equal(expectedPath, actualPath);
        }

        [Fact]
        public void Create()
        {
            var producer = InputProducer.Create();

            Assert.True(producer.FileCount > 0);
            string path = producer.GetPath(3.1);
            this.Output.WriteLine(path);
            Assert.True(File.Exists(path));
        }
    }
}