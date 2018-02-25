namespace ImageSharp.LoadTest
{
    using System.Globalization;

    class ExecutionParameters
    {
        public bool IsInteractive { get; private set; } = true;
        public int MeanImageWidth { get; private set; } = 3200;
        public int ImageWidthDeviation { get; private set; } = 1000;
        public int AverageMsBetweenRequests { get; private set; } = 200;
        public int NoOfReq { get; private set; } = 610;

        public static ExecutionParameters Parse(string[] args)
        {
            var result = new ExecutionParameters();
            result.ParseImpl(args);
            return result;
        }

        private void ParseImpl(string[] args)
        {
            if (args.Length > 3)
            {
                this.MeanImageWidth = int.Parse(args[1], CultureInfo.InvariantCulture);
                this.ImageWidthDeviation = int.Parse(args[2], CultureInfo.InvariantCulture);
                this.AverageMsBetweenRequests = int.Parse(args[3], CultureInfo.InvariantCulture);
            }

            this.IsInteractive = args.Length == 0 || args.Length > 4 && args[4].ToLower().StartsWith("i")
                                                  || args.Length > 5 && args[5].ToLower().StartsWith("i");

            if (args.Length > 4 && int.TryParse(args[4], out int n))
            {
                this.NoOfReq = n;
            }
        }

        public override string ToString()
        {
            return
                $"meanImageWidth={this.MeanImageWidth} | imageWidthDeviation={this.ImageWidthDeviation} | averageMsBetweenRequests={this.AverageMsBetweenRequests} | NumberOfRequests={this.NoOfReq}";
        }
    }
}