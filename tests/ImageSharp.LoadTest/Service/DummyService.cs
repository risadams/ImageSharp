namespace ImageSharp.LoadTest.Service
{
    using System;
    using System.Threading.Tasks;

    using ImageSharp.LoadTest.Statistics;

    public class DummyService : ITestService
    {
        public Task<ServiceInvocationReport> ProcessImage(Func<string> pathProducer)
        {
            var dummyReport = new ServiceInvocationReport(DateTime.Now, 0, 1);

            return Task.FromResult(dummyReport);
        }
    }
}