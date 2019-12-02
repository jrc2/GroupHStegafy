using System;
using System.Drawing;
using GroupHStegafy.Model;

namespace GroupHStegafy.Utilities
{
    public static class ImageUtilities
    {
        public const int BytesPerPixel = 4;

        public static void ChangePixelColor(byte[] imageData, int x, int y, int width, Color color)
        {
            const int alphaOffset = 3;
            var offset = CalculateOffset(x, y, width);
            imageData[offset + alphaOffset] = color.A;
            imageData[offset + PixelColorByteOffset(PixelColor.Red)] = color.R;
            imageData[offset + PixelColorByteOffset(PixelColor.Green)] = color.G;
            imageData[offset + PixelColorByteOffset(PixelColor.Blue)] = color.B;
        }

        public static byte GetByteForColor(byte[] imageData, int x, int y, int width, PixelColor pixelColor)
        {
            var offset = CalculateOffset(x, y, width);
            return imageData[offset + PixelColorByteOffset(pixelColor)];
        }

        public static int CalculateOffset(int x, int y, int width)
        {
            return (y * width + x) * BytesPerPixel;
        }

        public static void SetPixel(byte[] imageData, int x, int y, byte newPixel, int width)
        {
            // TODO check naming (is newPixel is not a pixel)
            var offset = CalculateOffset(x, y, width);
            imageData[offset + PixelColorByteOffset(PixelColor.Blue)] = newPixel;
        }

        public static bool IsPixelWhite(byte[] imageData, int x, int y, int width)
        {
            var offset = CalculateOffset(x, y, width);
            var r = imageData[offset + PixelColorByteOffset(PixelColor.Red)];
            var g = imageData[offset + PixelColorByteOffset(PixelColor.Green)];
            var b = imageData[offset + PixelColorByteOffset(PixelColor.Blue)];
            var pixelColor = Color.FromArgb(0, r, g, b);
            return pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255;
        }

        public static Color GetPixelColor(byte[] imageData, int x, int y, int width)
        {
            const int alphaOffset = 3;
            var offset = CalculateOffset(x, y, width);
            var alpha = imageData[offset + alphaOffset];
            var red = imageData[offset + PixelColorByteOffset(PixelColor.Red)];
            var green = imageData[offset + PixelColorByteOffset(PixelColor.Green)];
            var blue = imageData[offset + PixelColorByteOffset(PixelColor.Blue)];

            return Color.FromArgb(alpha, red, green, blue);
        }

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

        /// <summary>
        /// Gets the bits per color channel.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="width">The image width.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">width - must be at least 1</exception>
        public static int GetBpcc(byte[] imageData, int width)
        {
            if (width < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "must be at least 1");
            }

            var offset = CalculateOffset(1, 0, width);
            return imageData[offset + PixelColorByteOffset(PixelColor.Green)];
        }
    }
}
