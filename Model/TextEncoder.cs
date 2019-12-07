using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Controls;
using GroupHStegafy.Utilities;

namespace GroupHStegafy.Model
{
    /// <summary>
    ///     Handles encoding and decoding of text into an image.
    /// </summary>
    public class TextEncoder
    {
        private const int bpcc = 2;

        /// <summary>
        ///     Encodes the message.
        /// </summary>
        /// <param name="originalImageBytes">The original image bytes.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public byte[] EncodeMessage(byte[] originalImageBytes, string message)
        {
            throw new ArgumentException("Not Implemented");
        }

        /// <summary>
        ///     Decodes the message.
        /// </summary>
        /// <param name="modifiedImageBytes">The modified image bytes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Not Implemented</exception>
        public string DecodeMessage(byte[] modifiedImageBytes, int modifiedImageHeight, int modifiedImageWidth)
        {
            var leastSignificantBits = new List<bool>();

            for (var y = 0; y < modifiedImageHeight; y++)
            {
                for (var x = 0; x < modifiedImageWidth; x++)
                {
                    var currentPixelData = ImageUtilities.GetPixelBytes(modifiedImageBytes, x, y, modifiedImageWidth);
                    foreach (var aByte in currentPixelData)
                    {
                        leastSignificantBits.AddRange(convertByteToBoolArray(aByte).Skip(8-bpcc).Take(bpcc));
                    }
                }
            }

            var stringData = convertBoolArrayToByteArray(leastSignificantBits.ToArray());
            return Encoding.UTF8.GetString(stringData, 0, stringData.Length);
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

        private byte[] convertBoolArrayToByteArray(bool[] bits)
        {
            if (bits.Length % 8 != 0)
            {
                throw new ArgumentException("Invalid bits");
            }
            var byteArray = new byte[bits.Length / 8];
            for (var i = 0; i < bits.Length; i+=8)
            {
                var newByteData = bits.Skip(i).Take(8).ToArray();
                byteArray[i / 8] = ConvertBoolArrayToByte(newByteData);
            }

            return byteArray;
        }

        private static byte ConvertBoolArrayToByte(bool[] source)
        {
            if (source.Length != 8)
            {
                throw new ArgumentException("Invalid byte data.");
            } 

            byte result = 0;
            int index = 8 - source.Length;

            foreach (bool b in source)
            {
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }
    }
}
