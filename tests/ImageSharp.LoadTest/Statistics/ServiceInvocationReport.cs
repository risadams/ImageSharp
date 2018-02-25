namespace ImageSharp.LoadTest.Statistics
{
    using System;

    public class ServiceInvocationReport
    {
        public ServiceInvocationReport(DateTime startTime, double megaPixelsProcessed, long milliseconds)
        {
            this.StartTime = startTime;
            
            this.MegaPixelsProcessed = megaPixelsProcessed;
            this.Milliseconds = milliseconds;
        }

        public DateTime StartTime { get; }

        public double MegaPixelsProcessed { get; }
        public long Milliseconds { get; }

        public DateTime FinshedTime => this.StartTime.AddMilliseconds(this.Milliseconds);

        public override string ToString()
        {
            return $"{this.MegaPixelsProcessed:0.00}MP | {this.Milliseconds:0000}ms";
        }
    }
}