using System;
using System.Drawing;
using GroupHStegafy.Model;

namespace GroupHStegafy.Utilities
{
    /// <summary>
    ///     Provides Basic Utility for modifying an image for Steganography. 
    /// </summary>
    public static class ImageUtilities
    {
        private const PixelColor LeastSignificantPixelColor = PixelColor.Blue;
        private const int BytesPerPixel = 4;

        /// <summary>
        ///     Replaces the least significant bit.
        /// </summary>
        /// <param name="originalBytes">The original bytes.</param>
        /// <param name="originalImageWidth">Width of the original image.</param>
        /// <param name="secretImageBytes">The secret image bytes.</param>
        /// <param name="secretImageWidth">Width of the secret image.</param>
        /// <param name="secretImageHeight">Height of the secret image.</param>
        /// <returns></returns>
        public static byte[] ReplaceLeastSignificantBit(byte[] originalBytes, int originalImageWidth, byte[] secretImageBytes, int secretImageWidth, int secretImageHeight)
        {
            for (var x = 0; x < secretImageHeight; x++)
            {
                for (var y = 0; y < secretImageWidth; y++)
                {
                    var newOriginalImagePixel = getPixel(originalBytes, x, y, originalImageWidth);
                    if (isPixelWhite(secretImageBytes, x, y, secretImageWidth))
                    {
                        newOriginalImagePixel |= 1;
                    }
                    else
                    {
                        newOriginalImagePixel &= 0xfe;
                    }
                    setPixel(originalBytes, x, y, newOriginalImagePixel, originalImageWidth);
                }
            }

            return originalBytes;
        }

        private static byte getPixel(byte[] pixels, int x, int y, int width)
        {
            var offset = (x * width + y) * BytesPerPixel;
            return pixels[offset + pixelColorByteOffset(LeastSignificantPixelColor)];
        }

        private static bool isPixelWhite(byte[] pixels, int x, int y, int width)
        {
            var offset = (x * width + y) * BytesPerPixel;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            var pixelColor = Color.FromArgb(0, r, g, b);
            return pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255;
        }

        private static void setPixel(byte[] pixels, int x, int y, byte newPixel, int width)
        {
            var offset = (x * width + y) * BytesPerPixel;
            pixels[offset] = newPixel;
        }

        public static byte[] ReadLeastSignificantBits(byte[] modifiedImageBytes, int modifiedImageWidth, int modifiedImageHeight)
        {
            if (modifiedImageBytes.Length % BytesPerPixel != 0)
            {
                throw new ArgumentException("Invalid Byte Array");
            }

            var secretImageBytes = new byte[modifiedImageBytes.Length];

            for (var x = 0; x < modifiedImageHeight; x++)
            {
                for (var y = 0; y < modifiedImageWidth; y++)
                {
                    if (isInsignificantBit(getPixel(modifiedImageBytes, x, y, modifiedImageWidth)))
                    {
                        addWhitePixel(secretImageBytes, x, y, modifiedImageWidth);
                    }
                    else
                    {
                        addBlackPixel(secretImageBytes, x, y, modifiedImageWidth);
                    }

                }
            }

            return secretImageBytes;
        }

        private static bool isInsignificantBit(byte insignificantBit)
        {
            return convertByteToBoolArray(insignificantBit)[7];
        }

        private static bool[] convertByteToBoolArray(byte aByte)
        {
            var result = new bool[8];

            for (var i = 0; i < 8; i++)
            {
                result[i] = (aByte & (1 << i)) != 0;
            }
            Array.Reverse(result);

            return result;
        }

        private static void addWhitePixel(byte[] pixels, int x, int y, int width)
        {
            var offset = (x * width + y) * BytesPerPixel;
            pixels[offset + 3] = 255;
            pixels[offset + 2] = 255;
            pixels[offset + 1] = 255;
            pixels[offset + 0] = 255;
        }

        private static void addBlackPixel(byte[] pixels, int x, int y, int width)
        {
            var offset = (x * width + y) * BytesPerPixel;
            pixels[offset + 3] = 255;
            pixels[offset + 2] = 0;
            pixels[offset + 1] = 0;
            pixels[offset + 0] = 0;
        }

        private static int pixelColorByteOffset(PixelColor color)
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
