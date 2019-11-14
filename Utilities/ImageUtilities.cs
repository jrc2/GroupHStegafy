using System;
using System.Collections;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupHStegafy.Utility
{
    public static class ImageUtilities
    {
        private const int LeastSignificantByte = 3;
        private const int LeastSignificantBitInByte = 7;

        /// <summary>
        ///     Returns a Byte Array of the Image Data in an Image File.
        /// </summary>
        /// <param name="imageFile">The image file.</param>
        /// <returns>a Byte Array of the Image Data</returns>
        public static async Task<byte[]> GetImageBytes(StorageFile imageFile)
        {
            var copyBitmapImage = await makeACopyOfTheFileToWorkOn(imageFile);

            using var fileStream = await imageFile.OpenAsync(FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(fileStream);
            var transform = new BitmapTransform
            {
                ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
            };

            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );

            return pixelData.DetachPixelData();
        }

        private static async Task<BitmapImage> makeACopyOfTheFileToWorkOn(IRandomAccessStreamReference imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        /// <summary>
        ///     Replaces the last bit of every third byte in originalBytes
        ///     with the next bit in newBits.
        /// </summary>
        /// <param name="originalBytes">The original bytes.</param>
        /// <param name="newBits">The new bits</param>
        /// <returns>The new OriginalByteArray</returns>
        public static byte[] ReplaceLeastSignificantBit(byte[] originalBytes, BitArray newBits)
        {
            if (newBits.Length * 3 > originalBytes.Length)
            {
                throw new ArgumentException("Not Enough Bytes in originalBytes");
            }

            for (var i = 0; i < originalBytes.Length; i++)
            {
                originalBytes[i] = BitArrayToByte(new BitArray(originalBytes[i * 3]) { [LeastSignificantBitInByte] = newBits[i] });
            }

            return originalBytes;
        }

        /// <summary>
        ///     Returns a BitArray of the LeastSignificantBits of a Byte Array.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns> a BitArray of the LeastSignificantBits</returns>
        /// <exception cref="ArgumentException">Invalid Byte Array</exception>
        public static BitArray ReadLeastSignificantBits(byte[] bytes)
        {
            if (bytes.Length % 3 != 0)
            {
                throw new ArgumentException("Invalid Byte Array");
            }

            var bitArray = new BitArray(bytes.Length / 3);
            for (var i = 0; i < bytes.Length; i++)
            {
                bitArray[i] = new BitArray(bytes[i * 3])[LeastSignificantBitInByte];
            }

            return bitArray;
        }

        /// <summary>
        ///     Converts a bitArray to a Byte.
        /// </summary>
        /// <param name="bits">The bit array.</param>
        /// <returns>The Byte created from the bit array.</returns>
        /// <exception cref="ArgumentException">bits</exception>
        public static byte BitArrayToByte(BitArray bits)
        {
            if (bits.Count != 8)
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
                var currentByteBitArray = new BitArray(8);
                for (var j = 0; j < 8; j++)
                {
                    currentByteBitArray[j] = bits[i * 8 + j];
                }

                bytes[i] = BitArrayToByte(currentByteBitArray);
            }

            return bytes;
        }
    }
}
