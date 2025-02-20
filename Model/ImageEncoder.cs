﻿using System;
using System.Drawing;
using GroupHStegafy.Utilities;

namespace GroupHStegafy.Model
{
    /// <summary>
    ///     Provides Basic Utility for modifying an image for Steganography.
    /// </summary>
    public class ImageEncoder
    {
        #region Data members

        private const PixelColor LeastSignificantPixelColor = PixelColor.Blue;
        private const int LeastSignificantBit = 7;

        #endregion

        #region Methods

        /// <summary>
        ///     Returns a byte array of image data the is the modified image data.
        /// </summary>
        /// <param name="originalBytes">The original bytes.</param>
        /// <param name="originalImageWidth">Width of the original image.</param>
        /// <param name="secretImageBytes">The secret image bytes.</param>
        /// <param name="secretImageWidth">Width of the secret image.</param>
        /// <param name="secretImageHeight">Height of the secret image.</param>
        /// <returns></returns>
        public byte[] EncodeImage(byte[] originalBytes, int originalImageWidth, byte[] secretImageBytes,
            int secretImageWidth, int secretImageHeight)
        {
            if (originalBytes.Length < secretImageBytes.Length || originalImageWidth < secretImageWidth)
            {
                throw new ArgumentException("Original Image cannot contain Secret Image.");
            }

            for (var y = 0; y < secretImageHeight; y++)
            {
                for (var x = 0; x < secretImageWidth; x++)
                {
                    var originalLsb = ImageUtilities.GetByteForColor(originalBytes, x, y, originalImageWidth,
                        LeastSignificantPixelColor);
                    if (ImageUtilities.IsPixelWhite(secretImageBytes, x, y, secretImageWidth))
                    {
                        originalLsb |= 1;
                    }
                    else
                    {
                        originalLsb &= 0xfe;
                    }

                    ImageUtilities.SetPixel(originalBytes, x, y, originalLsb, originalImageWidth, PixelColor.Blue);
                }
            }

            return originalBytes;
        }

        /// <summary>
        ///     Returns a byte array of the image data of the secret message bitmap.
        /// </summary>
        /// <param name="modifiedImageBytes">The modified image bytes.</param>
        /// <param name="modifiedImageWidth">Width of the modified image.</param>
        /// <param name="modifiedImageHeight">Height of the modified image.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Invalid ModifiedImageBytes</exception>
        public byte[] DecodeImage(byte[] modifiedImageBytes, int modifiedImageWidth, int modifiedImageHeight)
        {
            if (modifiedImageBytes.Length % ImageUtilities.BytesPerPixel != 0)
            {
                throw new ArgumentException("Invalid Modified Image");
            }

            var secretImageBytes = new byte[modifiedImageBytes.Length];

            for (var y = 0; y < modifiedImageHeight; y++)
            {
                for (var x = 0; x < modifiedImageWidth; x++)
                {
                    if (this.isInsignificantBit(ImageUtilities.GetByteForColor(modifiedImageBytes, x, y,
                        modifiedImageWidth, LeastSignificantPixelColor)))
                    {
                        ImageUtilities.ChangePixelColor(secretImageBytes, x, y, modifiedImageWidth, Color.White);
                    }
                    else
                    {
                        ImageUtilities.ChangePixelColor(secretImageBytes, x, y, modifiedImageWidth, Color.Black);
                    }
                }
            }

            return secretImageBytes;
        }

        public static byte[] EncryptImage(byte[] secretImageBytes, int originalImageWidth, int originalImageHeight,
            int secretImageWidth, int secretImageHeight)
        {
            var expandedSecretImage = ImageUtilities.ExpandSecretImage(secretImageBytes, originalImageWidth,
                originalImageHeight,
                secretImageWidth, secretImageHeight);

            return ImageUtilities.SwitchImageHalves(expandedSecretImage);
        }

        private bool isInsignificantBit(byte insignificantBit)
        {
            return this.convertByteToBoolArray(insignificantBit)[LeastSignificantBit];
        }

        private bool[] convertByteToBoolArray(byte aByte)
        {
            var result = new bool[8];

            for (var i = 0; i < 8; i++)
            {
                result[i] = (aByte & (1 << i)) != 0;
            }

            Array.Reverse(result);

            return result;
        }

        #endregion
    }
}