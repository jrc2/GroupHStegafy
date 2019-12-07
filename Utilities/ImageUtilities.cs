using System;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using GroupHStegafy.Model;

namespace GroupHStegafy.Utilities
{
    /// <summary>
    ///     Provides basic Image editing utilities.
    /// </summary>
    public static class ImageUtilities
    {
        /// <summary>
        ///     The bytes per pixel
        /// </summary>
        public const int BytesPerPixel = 4;

        /// <summary>
        ///     Changes the color of the pixel.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="color">The color.</param>
        public static void ChangePixelColor(byte[] imageData, int x, int y, int width, Color color)
        {
            const int alphaOffset = 3;
            var offset = CalculateByteOffset(x, y, width);
            imageData[offset + alphaOffset] = color.A;
            imageData[offset + PixelColorByteOffset(PixelColor.Red)] = color.R;
            imageData[offset + PixelColorByteOffset(PixelColor.Green)] = color.G;
            imageData[offset + PixelColorByteOffset(PixelColor.Blue)] = color.B;
        }

        /// <summary>
        ///     Gets the color of the byte for.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="pixelColor">Color of the pixel.</param>
        /// <returns></returns>
        public static byte GetByteForColor(byte[] imageData, int x, int y, int width, PixelColor pixelColor)
        {
            var offset = CalculateByteOffset(x, y, width);
            return imageData[offset + PixelColorByteOffset(pixelColor)];
        }

        public static byte[] GetPixelBytes(byte[] imageData, int x, int y, int width)
        {
            var pixelData = new byte[4];

            var offset = CalculateByteOffset(x, y, width);
            pixelData[0] = imageData[offset];
            pixelData[1] = imageData[offset + 1];
            pixelData[2] = imageData[offset + 2];
            pixelData[3] = imageData[offset + 3];

            return pixelData;
        }

        /// <summary>
        ///     Calculates the offset.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        public static int CalculateByteOffset(int x, int y, int width)
        {
            return (y * width + x) * BytesPerPixel;
        }

        /// <summary>
        ///     Sets the pixel.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="newPixel">The new pixel.</param>
        /// <param name="width">The width.</param>
        public static void SetPixel(byte[] imageData, int x, int y, byte newPixel, int width)
        {
            // TODO check naming (is newPixel is not a pixel)
            var offset = CalculateByteOffset(x, y, width);
            imageData[offset + PixelColorByteOffset(PixelColor.Blue)] = newPixel;
        }

        /// <summary>
        ///     Determines whether the pixel is white.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <returns>
        ///   <c>true</c> if [is pixel white] [the specified image data]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPixelWhite(byte[] imageData, int x, int y, int width)
        {
            var offset = CalculateByteOffset(x, y, width);
            var r = imageData[offset + PixelColorByteOffset(PixelColor.Red)];
            var g = imageData[offset + PixelColorByteOffset(PixelColor.Green)];
            var b = imageData[offset + PixelColorByteOffset(PixelColor.Blue)];
            var pixelColor = Color.FromArgb(0, r, g, b);
            return pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255;
        }

        /// <summary>
        ///     Gets the color of the pixel.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        public static Color GetPixelColor(byte[] imageData, int x, int y, int width)
        {
            const int alphaOffset = 3;
            var offset = CalculateByteOffset(x, y, width);
            var alpha = imageData[offset + alphaOffset];
            var red = imageData[offset + PixelColorByteOffset(PixelColor.Red)];
            var green = imageData[offset + PixelColorByteOffset(PixelColor.Green)];
            var blue = imageData[offset + PixelColorByteOffset(PixelColor.Blue)];

            return Color.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        ///     Pixels the color byte offset.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static int PixelColorByteOffset(PixelColor color)
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
        ///     Reads the image.
        /// </summary>
        /// <param name="sourceImageFile">The source image file.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Invalid File.</exception>
        public static async Task<WriteableBitmap> ReadImage(StorageFile sourceImageFile)
        {
            if (!sourceImageFile.IsAvailable)
            {
                throw new ArgumentException("Invalid File.");
            }

            var copyBitmapImage = await MakeACopyOfTheFileToWorkOn(sourceImageFile);

            using var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read);

            var decoder = await BitmapDecoder.CreateAsync(fileStream);
            var transform = new BitmapTransform
            {
                ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
            };

            var image = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );

            var sourcePixels = pixelData.DetachPixelData();

            using var writeStream = image.PixelBuffer.AsStream();
            await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);

            return image;
        }

        /// <summary>
        ///     Makes a copy of the file to work on.
        /// </summary>
        /// <param name="imageFile">The image file.</param>
        /// <returns></returns>
        public static async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        /// <summary>
        ///     Gets the image data.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        public static async Task<byte[]> GetImageData(WriteableBitmap bitmap)
        {
            using var stream = bitmap.PixelBuffer.AsStream();
            var imageData = new byte[(uint)stream.Length];
            await stream.ReadAsync(imageData, 0, imageData.Length);

            return imageData;
        }
    }
}
