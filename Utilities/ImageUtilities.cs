using System;
using System.Drawing;
using GroupHStegafy.Model;

namespace GroupHStegafy.Utilities
{
    /// <summary>
    ///     Provides basic Image editing utilities.
    /// </summary>
    public static class ImageUtilities
    {
        /// <summary>
        ///     The bytes per pixel
        /// </summary>
        public const int BytesPerPixel = 4;

        /// <summary>
        ///     Changes the color of the pixel.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="color">The color.</param>
        public static void ChangePixelColor(byte[] imageData, int x, int y, int width, Color color)
        {
            const int alphaOffset = 3;
            var offset = CalculateByteOffset(x, y, width);
            imageData[offset + alphaOffset] = color.A;
            imageData[offset + PixelColorByteOffset(PixelColor.Red)] = color.R;
            imageData[offset + PixelColorByteOffset(PixelColor.Green)] = color.G;
            imageData[offset + PixelColorByteOffset(PixelColor.Blue)] = color.B;
        }

        /// <summary>
        ///     Gets the color of the byte for.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="pixelColor">Color of the pixel.</param>
        /// <returns></returns>
        public static byte GetByteForColor(byte[] imageData, int x, int y, int width, PixelColor pixelColor)
        {
            var offset = CalculateByteOffset(x, y, width);
            return imageData[offset + PixelColorByteOffset(pixelColor)];
        }

        /// <summary>
        ///     Calculates the offset.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        public static int CalculateByteOffset(int x, int y, int width)
        {
            return (y * width + x) * BytesPerPixel;
        }

        /// <summary>
        ///     Sets the pixel.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="newPixel">The new pixel.</param>
        /// <param name="width">The width.</param>
        public static void SetPixel(byte[] imageData, int x, int y, byte newPixel, int width)
        {
            // TODO check naming (is newPixel is not a pixel)
            var offset = CalculateByteOffset(x, y, width);
            imageData[offset + PixelColorByteOffset(PixelColor.Blue)] = newPixel;
        }

        /// <summary>
        ///     Determines whether the pixel is white.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <returns>
        ///   <c>true</c> if [is pixel white] [the specified image data]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPixelWhite(byte[] imageData, int x, int y, int width)
        {
            var offset = CalculateByteOffset(x, y, width);
            var r = imageData[offset + PixelColorByteOffset(PixelColor.Red)];
            var g = imageData[offset + PixelColorByteOffset(PixelColor.Green)];
            var b = imageData[offset + PixelColorByteOffset(PixelColor.Blue)];
            var pixelColor = Color.FromArgb(0, r, g, b);
            return pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255;
        }

        /// <summary>
        ///     Gets the color of the pixel.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        public static Color GetPixelColor(byte[] imageData, int x, int y, int width)
        {
            const int alphaOffset = 3;
            var offset = CalculateByteOffset(x, y, width);
            var alpha = imageData[offset + alphaOffset];
            var red = imageData[offset + PixelColorByteOffset(PixelColor.Red)];
            var green = imageData[offset + PixelColorByteOffset(PixelColor.Green)];
            var blue = imageData[offset + PixelColorByteOffset(PixelColor.Blue)];

            return Color.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        ///     Pixels the color byte offset.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static int PixelColorByteOffset(PixelColor color)
        {
            return color switch
            {
                PixelColor.Blue => 0,
                PixelColor.Green => 1,
                PixelColor.Red => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Invalid Color.")
            };
        }
    }
}
