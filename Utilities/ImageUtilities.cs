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
            for (var y = 0; y < secretImageHeight; y++)
            {
                for (var x = 0; x < secretImageWidth; x++)
                {
                    var originalLsb = getByteForColor(originalBytes, x, y, originalImageWidth, LeastSignificantPixelColor);
                    if (isPixelWhite(secretImageBytes, x, y, secretImageWidth))
                    {
                        originalLsb |= 1;
                    }
                    else
                    {
                        originalLsb &= 0xfe;
                    }
                    setPixel(originalBytes, x, y, originalLsb, originalImageWidth);
                }
            }

            return originalBytes;
        }

        private static int calculateOffset(int x, int y, int width)
        {
            return (y * width + x) * BytesPerPixel;
        }

        private static byte getByteForColor(byte[] imageData, int x, int y, int width, PixelColor pixelColor)
        {
            var offset = calculateOffset(x, y, width);
            return imageData[offset + pixelColorByteOffset(pixelColor)];
        }

        private static bool isPixelWhite(byte[] imageData, int x, int y, int width)
        {
            var offset = calculateOffset(x, y, width);
            var r = imageData[offset + pixelColorByteOffset(PixelColor.Red)];
            var g = imageData[offset + pixelColorByteOffset(PixelColor.Green)];
            var b = imageData[offset + pixelColorByteOffset(PixelColor.Blue)];
            var pixelColor = Color.FromArgb(0, r, g, b);
            return pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255;
        }

        private static void setPixel(byte[] imageData, int x, int y, byte newPixel, int width)
        {
            // TODO check naming (is newPixel is not a pixel)
            var offset = calculateOffset(x, y, width);
            imageData[offset + pixelColorByteOffset(PixelColor.Blue)] = newPixel;
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

            for (var y = 0; y < modifiedImageHeight; y++)
            {
                for (var x = 0; x < modifiedImageWidth; x++)
                {
                    if (isInsignificantBit(getByteForColor(modifiedImageBytes, x, y, modifiedImageWidth, LeastSignificantPixelColor)))
                    {
                        changePixelColor(secretImageBytes, x, y, modifiedImageWidth, Color.White);
                    }
                    else
                    {
                        changePixelColor(secretImageBytes, x, y, modifiedImageWidth, Color.Black);
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

        private static void changePixelColor(byte[] imageData, int x, int y, int width, Color color)
        {
            const int alphaOffset = 3;
            var offset = calculateOffset(x, y, width);
            imageData[offset + alphaOffset] = color.A;
            imageData[offset + pixelColorByteOffset(PixelColor.Red)] = color.R;
            imageData[offset + pixelColorByteOffset(PixelColor.Green)] = color.G;
            imageData[offset + pixelColorByteOffset(PixelColor.Blue)] = color.B;
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

        /// <summary>
        /// Adds the image header.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="isEncrypted">if set to <c>true</c> [is encrypted].</param>
        /// <param name="messageType">Type of the message.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">imageWidth - must be at least 1</exception>
        public static byte[] AddHeader(byte[] imageData, int imageWidth, bool isEncrypted, MessageType messageType)
        {
            if (imageWidth < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(imageWidth), "must be at least 1");
            }

            changePixelColor(imageData, 0, 0, imageWidth, Color.FromArgb(212, 212, 212));

            var offset = calculateOffset(1, 0, imageWidth);
            imageData[offset + pixelColorByteOffset(PixelColor.Green)] = BytesPerPixel; //TODO should be bits per color channel

            var secondPixelRedByte = getByteForColor(imageData, 1, 0, imageWidth, PixelColor.Red);
            var secondPixelBlueByte = getByteForColor(imageData, 1, 0, imageWidth, PixelColor.Blue);

            if (isEncrypted)
            {
                secondPixelRedByte |= 1;
            }
            else
            {
                secondPixelRedByte &= 0xfe;
            }

            if (messageType == MessageType.Text)
            {
                secondPixelBlueByte |= 1;
            }
            else
            {
                secondPixelBlueByte &= 0xfe;
            }

            setPixel(imageData, 1, 0, secondPixelRedByte, imageWidth);
            setPixel(imageData, 1, 0, secondPixelBlueByte, imageWidth);

            return imageData;
        }
    }

    /// <summary>
    /// Embedded message types
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Text message type
        /// </summary>
        Text,
        /// <summary>
        /// Monochrome BMP message type
        /// </summary>
        MonochromeBmp
    }
}
