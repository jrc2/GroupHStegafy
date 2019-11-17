using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupHStegafy.Controllers
{
    /// <summary>
    ///     Handles Reading, Modifying, and Saving Images used for Steganography.
    /// </summary>
    public class ImageManager
    {
        /// <summary>
        ///     The original Image before it is modified.
        /// </summary>
        public WriteableBitmap OriginalImage;

        /// <summary>
        ///     The original Image after it is modified.
        /// </summary>
        public WriteableBitmap ModifiedImage;

        public ImageManager()
        {
            this.OriginalImage = null;
            this.ModifiedImage = null;
        }

        public async Task ReadImage(StorageFile sourceImageFile)
        {
            if (!sourceImageFile.IsAvailable)
            {
                throw new ArgumentException("Invalid File.");
            }

            var copyBitmapImage = await makeACopyOfTheFileToWorkOn(sourceImageFile);

            using var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read);

            var decoder = await BitmapDecoder.CreateAsync(fileStream);
            var transform = new BitmapTransform {
                ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
            };

            this.OriginalImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);

            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );

            var sourcePixels = pixelData.DetachPixelData();

            using var writeStream = this.OriginalImage.PixelBuffer.AsStream();
            await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
        }


        public async Task SaveImage(StorageFile saveFile)
        {
            if (saveFile == null)
            {
                throw new ArgumentException("Invalid SaveFile.");
            }

            var stream = await saveFile.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

            var pixelStream = this.ModifiedImage.PixelBuffer.AsStream();
            var pixels = new byte[pixelStream.Length];
            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                (uint)this.ModifiedImage.PixelWidth,
                (uint)this.ModifiedImage.PixelHeight, this.ModifiedImage.PixelHeight, this.ModifiedImage.PixelWidth, pixels);
            await encoder.FlushAsync();

            stream.Dispose();
        }

        private static async Task<BitmapImage> makeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

    }
}
