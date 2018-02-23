namespace ImageSharp.LoadTest.Tests
{
    using System.Threading.Tasks;

    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.Random;

    using Xunit;
    using Xunit.Abstractions;

    public class ResizeServiceTests
    {
        public ResizeServiceTests(ITestOutputHelper output)
        {
            this.Output = output;
        }

        private ResizeService Service { get; } = new ResizeService();

        private ITestOutputHelper Output { get; }

        [Fact]
        public async Task ProcessImage()
        {
            var dist = new Normal(3.0, 0.1, new Mrg32k3a(42));

            var inputProducer = InputProducer.Create(dist);
            string path = inputProducer.Next();

            this.Output.WriteLine(path);

            ServiceInvocationReport r = await this.Service.ProcessImage(() => path);
            this.Output.WriteLine(r.ToString());
            Assert.True(r.Milliseconds > 0);
        }

    }
}