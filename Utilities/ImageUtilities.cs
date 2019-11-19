using System;
using System.Collections;
using GroupHStegafy.Model;

namespace GroupHStegafy.Utilities
{
    /// <summary>
    ///     Provides Basic Utility for modifying an image for Steganography. 
    /// </summary>
    public static class ImageUtilities
    {
        private const PixelColor LeastSignificantPixelColor = PixelColor.Blue;
        private const int LeastSignificantBitInByte = 7;
        private const int BytesPerPixel = 3;

        /// <summary>
        ///     Replaces the last bit of every third byte in originalBytes
        ///     with the next bit in newBits.
        /// </summary>
        /// <param name="originalBytes">The original bytes.</param>
        /// <param name="newBits">The new bits</param>
        /// <returns>The new OriginalByteArray</returns>
        public static byte[] ReplaceLeastSignificantBit(byte[] originalBytes, BitArray newBits)
        {
            //TODO: Correctly implement
            if (newBits.Length * BytesPerPixel > originalBytes.Length)
            {
                throw new ArgumentException("Not Enough Bytes in originalBytes");
            }

            for (var i = 0; i < newBits.Length - 1; i++)
            {
                originalBytes[i * BytesPerPixel] = replaceInsignificantBit(originalBytes[i * BytesPerPixel], newBits[i]);
            }

            return originalBytes;
        }

        /// <summary>
        ///     Returns a BitArray of the LeastSignificantBits of a Byte Array.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns> a BitArray of the LeastSignificantBits</returns>
        /// <exception cref="ArgumentException">Invalid Byte Array</exception>
        public static byte[] ReadLeastSignificantBits(byte[] bytes)
        {
            //TODO: Correctly implement
            if (bytes.Length % BytesPerPixel != 0)
            {
                throw new ArgumentException("Invalid Byte Array");
            }

            var bitArray = new BitArray(bytes.Length / BytesPerPixel);
            for (var i = 0; i < bytes.Length; i++)
            {
                bitArray[i] = new BitArray(bytes[i * BytesPerPixel - pixelColorByteOffset(LeastSignificantPixelColor)])[LeastSignificantBitInByte];
            }

            return BitArrayToByteArray(bitArray);
        }

        private static byte replaceInsignificantBit(byte aByte, bool bit)
        {
            var boolArray = convertByteToBoolArray(aByte);
            boolArray[LeastSignificantBitInByte] = bit;
            return convertBoolArrayToByte(boolArray);
        }

        private static byte bitArrayToByte(BitArray bits)
        {
            if (bits.Length != 8)
            {
                throw new ArgumentException("Incorrect number of bits.");
            }

            var bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        /// <summary>
        ///     Converts an Array of Bits to an Array of Bytes.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <returns>An Array of Bytes.</returns>
        /// <exception cref="ArgumentException">Invalid BitArray</exception>
        public static byte[] BitArrayToByteArray(BitArray bits)
        {
            if (bits.Length % 8 != 0)
            {
                throw new ArgumentException("Invalid BitArray");
            }

            var bytes = new byte[bits.Length / 8];
            for (var i = 0; i < bytes.Length; i++)
            {
                var currentByteBitArray = new bool[8];
                for (var j = 0; j < 8; j++)
                {
                    currentByteBitArray[j] = bits[i * 8 + j];
                }

                bytes[i] = convertBoolArrayToByte(currentByteBitArray);
            }

            return bytes;
        }

        private static int pixelColorByteOffset(PixelColor color)
        {
            return color switch {
                PixelColor.Blue => 0,
                PixelColor.Green => 1,
                PixelColor.Red => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Invalid Color.")
            };
        }

        private static byte convertBoolArrayToByte(bool[] boolArray)
        {
            if (boolArray.Length > 8)
            {
                throw new ArgumentException("Invalid bool array.");
            }

            byte result = 0;
            var index = 8 - boolArray.Length;

            foreach (var bit in boolArray)
            {
                if (bit)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
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
    }
}
