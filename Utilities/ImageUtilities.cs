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
        private const int LeastSignificantBit = 7;
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
            var r = pixels[offset + pixelColorByteOffset(PixelColor.Red)];
            var g = pixels[offset + pixelColorByteOffset(PixelColor.Green)];
            var b = pixels[offset + pixelColorByteOffset(PixelColor.Blue)];
            var pixelColor = Color.FromArgb(0, r, g, b);
            return pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255;
        }

        private static void setPixel(byte[] pixels, int x, int y, byte newPixel, int width)
        {
            var offset = (x * width + y) * BytesPerPixel;
            pixels[offset + pixelColorByteOffset(PixelColor.Blue)] = newPixel;
        }

        /// <summary>
        ///     Returns a byte array of the image data of the secret message bitmap.
        /// </summary>
        /// <param name="modifiedImageBytes">The modified image bytes.</param>
        /// <param name="modifiedImageWidth">Width of the modified image.</param>
        /// <param name="modifiedImageHeight">Height of the modified image.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Invalid ModifiedImageBytes</exception>
        public static byte[] ReadLeastSignificantBits(byte[] modifiedImageBytes, int modifiedImageWidth, int modifiedImageHeight)
        {
            if (modifiedImageBytes.Length % BytesPerPixel != 0)
            {
                throw new ArgumentException("Invalid ModifiedImageBytes");
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
            return convertByteToBoolArray(insignificantBit)[LeastSignificantBit];
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
            const int alphaOffset = 3;
            var offset = (x * width + y) * BytesPerPixel;
            pixels[offset + alphaOffset] = 255;
            pixels[offset + pixelColorByteOffset(PixelColor.Red)] = 255;
            pixels[offset + pixelColorByteOffset(PixelColor.Green)] = 255;
            pixels[offset + pixelColorByteOffset(PixelColor.Blue)] = 255;
        }

        private static void addBlackPixel(byte[] pixels, int x, int y, int width)
        {
            const int alphaOffset = 3;
            var offset = (x * width + y) * BytesPerPixel;
            pixels[offset + alphaOffset] = 255;
            pixels[offset + pixelColorByteOffset(PixelColor.Red)] = 0;
            pixels[offset + pixelColorByteOffset(PixelColor.Green)] = 0;
            pixels[offset + pixelColorByteOffset(PixelColor.Blue)] = 0;
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
