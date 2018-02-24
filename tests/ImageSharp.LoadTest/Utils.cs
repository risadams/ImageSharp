namespace ImageSharp.LoadTest
{
    using SixLabors.ImageSharp;
    using SixLabors.Primitives;

    public static class Utils
    {
        public static double GetMegaPixels(this IImage image)
        {
            double size = image.Width * image.Height;
            return size / (1024 * 1024);
        }

        public static double GetMegaPixels(this Size size)
        {
            double s = size.Width * size.Height;
            return s / (1024 * 1024);
        }
    }
}