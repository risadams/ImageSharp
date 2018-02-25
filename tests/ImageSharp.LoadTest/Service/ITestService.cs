namespace ImageSharp.LoadTest.Service
{
    using System;
    using System.Threading.Tasks;

    using ImageSharp.LoadTest.Statistics;

    public interface ITestService
    {
        Task<ServiceInvocationReport> ProcessImage(Func<string> pathProducer);
    }
}