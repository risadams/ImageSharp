namespace ImageSharp.LoadTest
{
    using SixLabors.ImageSharp;
    using SixLabors.Primitives;

    public static class Utils
    {
        public static double GetMegaPixels(this Size size)
        {
            double s = size.Width * size.Height;
            return s / (1000 * 1000);
        }
    }
}