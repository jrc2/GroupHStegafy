using System;
using System.Collections.Generic;
using GroupHStegafy.Utilities;

namespace GroupHStegafy.Model
{
    /// <summary>
    ///     Handles encoding and decoding of text into an image.
    /// </summary>
    public class TextEncoder
    {
        private readonly int bitsPerColorChannel;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextEncoder"/> class.
        /// </summary>
        /// <param name="bitsPerColorChannel">The bits per color channel.</param>
        /// <exception cref="ArgumentException">Invalid BitsPerColorChannel</exception>
        public TextEncoder(int bitsPerColorChannel)
        {
            if (bitsPerColorChannel < 0 || bitsPerColorChannel > 8)
            {
                throw new ArgumentException("Invalid BitsPerColorChannel");
            }
            this.bitsPerColorChannel = bitsPerColorChannel;
        }

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
        public string DecodeMessage(byte[] modifiedImageBytes)
        {
            var secretMessage = "";

            var charBits = new bool[8];
            var bitCount = 0;
            for (var i = 8; i < modifiedImageBytes.Length; i+=ImageUtilities.BytesPerPixel)
            {
                for (int j = 2; j >= 0; j--)
                {
                    foreach (var bit in this.getInsignificantBits(modifiedImageBytes[i + j]))
                    {
                        charBits[bitCount] = bit;
                        bitCount++;
                        if (bitCount == 8)
                        {
                            var nextChar = Convert.ToChar(this.convertBoolArrayToByte(charBits));
                            secretMessage += nextChar;
                            bitCount = 0;
                        }

                        if (secretMessage.Contains("#.-.-.-#"))
                        {
                            return secretMessage.Replace("#.-.-.-#", "");
                        }
                    }
                }
            }

            return "Secret Message could not be read.";
        }

        private List<bool> getInsignificantBits(byte aByte)
        {
            var insignificantBits = new List<bool>();
            var byteBits = this.convertByteToBoolArray(aByte);
            for (var i = 7; i > 7 - this.bitsPerColorChannel; i--)
            {
                insignificantBits.Add(byteBits[i]);
            }

            return insignificantBits;
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

        private byte convertBoolArrayToByte(bool[] boolArray)
        {
            if (boolArray.Length != 8)
            {
                throw new ArgumentException("Invalid byte data.");
            } 

            byte result = 0;
            var index = 0;

            foreach (var bit in boolArray)
            {
                if (bit)
                {
                    result |= (byte)(1 << (7 - index));
                }

                index++;
            }

            return result;
        }
    }
}
