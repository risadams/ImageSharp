namespace ImageSharp.LoadTest
{
    using SixLabors.ImageSharp;

    public static class Utils
    {
        public static double GetMegaPixels(this IImage image)
        {
            double size = image.Width * image.Height;
            return size / (1024 * 1024);
        }
    }
}