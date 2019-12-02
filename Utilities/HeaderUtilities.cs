﻿using System;
using System.Drawing;
using Windows.Storage;
using GroupHStegafy.Model;

namespace GroupHStegafy.Utilities
{
    /// <summary>
    ///     Handles Reading and Writing Header information for modified files.
    /// </summary>
    public static class HeaderUtilities
    {
        /// <summary>
        /// Adds the image header.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="isEncrypted">if set to <c>true</c> [is encrypted].</param>
        /// <param name="bpcc">The BPCC.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">imageWidth - must be at least 1</exception>
        public static byte[] AddHeader(byte[] imageData, int imageWidth, bool isEncrypted, int bpcc, MessageType messageType)
        {
            if (imageWidth < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(imageWidth), "must be at least 1");
            }

            ImageUtilities.ChangePixelColor(imageData, 0, 0, imageWidth, Color.FromArgb(212, 212, 212));

            var offset = ImageUtilities.CalculateOffset(1, 0, imageWidth);
            imageData[offset + ImageUtilities.PixelColorByteOffset(PixelColor.Green)] = (byte)bpcc;

            var secondPixelRedByte = ImageUtilities.GetByteForColor(imageData, 1, 0, imageWidth, PixelColor.Red);
            var secondPixelBlueByte = ImageUtilities.GetByteForColor(imageData, 1, 0, imageWidth, PixelColor.Blue);

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

            ImageUtilities.SetPixel(imageData, 1, 0, secondPixelRedByte, imageWidth);
            ImageUtilities.SetPixel(imageData, 1, 0, secondPixelBlueByte, imageWidth);

            return imageData;
        }

        /// <summary>
        /// Determines whether the specified image data is encrypted.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="width">The image width.</param>
        /// <returns>
        ///   <c>true</c> if the specified image data is encrypted; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">width - must be at least 1</exception>
        public static bool IsEncrypted(byte[] imageData, int width)
        {
            if (width < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "must be at least 1");
            }

            var secondPixelRedByte = ImageUtilities.GetByteForColor(imageData, 1, 0, width, PixelColor.Red);
            secondPixelRedByte |= 0xFE;

            return secondPixelRedByte == 0xFF;
        }

        /// <summary>
        /// Gets the type of the embedded message.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="width">The image width.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">width - must be at least 1</exception>
        public static MessageType GetMessageType(byte[] imageData, int width)
        {
            if (width < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "must be at least 1");
            }

            var secondPixelBlueByte = ImageUtilities.GetByteForColor(imageData, 1, 0, width, PixelColor.Blue);
            secondPixelBlueByte |= 0xFE;

            if (secondPixelBlueByte == 0xFF)
            {
                return MessageType.Text;
            }

            return MessageType.MonochromeBmp;
        }

        /// <summary>
        /// Determines whether a message is embedded
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="width">The image width.</param>
        /// <returns>
        ///   <c>true</c> if message is embedded; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">width - must be at least 1</exception>
        public static bool IsMessageEmbedded(byte[] imageData, int width)
        {
            if (width < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "must be at least 1");
            }

            var color = ImageUtilities.GetPixelColor(imageData, 0, 0, width);
            return color == Color.FromArgb(212, 212, 212);
        }

        public static bool IsImageFile(StorageFile file)
        {
            //TODO: Implement
            return true;
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
