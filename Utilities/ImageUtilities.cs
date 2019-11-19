using System;
using System.Collections;
using System.Diagnostics;
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
        private const int BytesPerPixel = 4;

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
            if (newBits.Length > originalBytes.Length * 8)
            {
                throw new ArgumentException("Not Enough Bytes in originalBytes");
            }

            for (var i = 0; i < newBits.Length - 1; i++)
            {
                Debug.WriteLine(originalBytes[i * BytesPerPixel].ToString());
                originalBytes[i * BytesPerPixel] = replaceInsignificantBit(originalBytes[i * BytesPerPixel], newBits[i]);
                Debug.WriteLine(originalBytes[i * BytesPerPixel].ToString());
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

            var leastSignificantBits = new bool[bytes.Length];
            for (var i = 0; i < bytes.Length - 1; i+=BytesPerPixel)
            {
                leastSignificantBits[i] = 
                    convertByteToBoolArray(bytes[i + pixelColorByteOffset(LeastSignificantPixelColor)])[LeastSignificantBitInByte];
            }

            return getInsignificantByteArray(leastSignificantBits);
        }

        private static byte replaceInsignificantBit(byte aByte, bool bit)
        {
            var boolArray = convertByteToBoolArray(aByte);
            boolArray[LeastSignificantBitInByte] = bit;
            return convertBoolArrayToByte(boolArray);
        }

        private static byte convertBoolArrayToByte(bool[] boolArray)
        {
            if (boolArray.Length != 8)
            {
                throw new ArgumentException("Invalid bool array.");
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

        private static byte[] getInsignificantByteArray(bool[] insignificantBits)
        {
            var bytes = new byte[insignificantBits.Length];
            for (var i = 0; i < insignificantBits.Length - 1; i+=BytesPerPixel)
            {
                var newBoolArray = new bool[8];
                for (var j = 0; j < 8; j++)
                {
                    newBoolArray[j] = insignificantBits[i / BytesPerPixel];
                }

                bytes[  i  ] = convertBoolArrayToByte(newBoolArray);
                bytes[i + 1] = convertBoolArrayToByte(newBoolArray);
                bytes[i + 2] = convertBoolArrayToByte(newBoolArray);
                bytes[i + 3] = getAlphaByte();
            }

            return bytes;
        }

        private static byte getAlphaByte()
        {
            var newBoolArray = new bool[8];

            for (var i = 0; i < 8; i++)
            {
                newBoolArray[i] = true;
            }

            return convertBoolArrayToByte(newBoolArray);
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
