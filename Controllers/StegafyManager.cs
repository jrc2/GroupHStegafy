using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using GroupHStegafy.Model;
using GroupHStegafy.Utilities;

namespace GroupHStegafy.Controllers
{
    /// <summary>
    ///     Handles Reading, Modifying, and Saving Images used for Steganography.
    /// </summary>
    public class StegafyManager
    {
        /// <summary>
        ///     The original Image before it is modified.
        /// </summary>
        public WriteableBitmap OriginalImage;

        /// <summary>
        ///     The secret message to be embeded into the original image.
        /// </summary>
        public WriteableBitmap SecretImage;

        /// <summary>
        ///     The original Image after it is modified.
        /// </summary>
        public WriteableBitmap ModifiedImage;

        /// <summary>
        ///     The encrypted modified image
        /// </summary>
        public WriteableBitmap EncryptedModifiedImage;

        /// <summary>
        ///     The secret message
        /// </summary>
        public string SecretMessage;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StegafyManager"/> class.
        /// </summary>
        public StegafyManager()
        {
            this.OriginalImage = null;
            this.SecretImage = null;
            this.ModifiedImage = null;
            this.EncryptedModifiedImage = null;
            this.SecretMessage = null;
        }

        /// <summary>
        ///     Reads the original image.
        /// </summary>
        /// <param name="sourceImageFile">The source image file.</param>
        public async Task ReadOriginalImage(StorageFile sourceImageFile)
        {
            this.OriginalImage = await this.readImage(sourceImageFile);
        }

        /// <summary>
        ///     Reads the secret image.
        /// </summary>
        /// <param name="sourceImageFile">The source image file.</param>
        public async Task ReadSecretImage(StorageFile sourceImageFile)
        {
            this.SecretImage = await this.readImage(sourceImageFile);
        }

        /// <summary>
        ///     Reads the modified image.
        /// </summary>
        /// <param name="sourStorageFile">The sour storage file.</param>
        public async Task ReadModifiedImage(StorageFile sourStorageFile)
        {
            this.ModifiedImage = await this.readImage(sourStorageFile);
        }

        private async Task<WriteableBitmap> readImage(StorageFile sourceImageFile)
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

            var image = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);

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
        ///     Reads the secret message from text file.
        /// </summary>
        /// <param name="sourceTextFile">The source text file.</param>
        public async Task ReadSecretMessageFromTextFile(StorageFile sourceTextFile)
        {
            this.SecretMessage = await FileIO.ReadTextAsync(sourceTextFile);
        }

        /// <summary>
        ///     Embeds the secret image in the OriginalImage.
        /// </summary>
        public async Task EmbedSecretImage()
        {
            var secretImageData = await this.getImageData(this.SecretImage);
            var originalImageData = await this.getImageData(this.OriginalImage);

            var imageEncoder = new ImageEncoder();

            var modifiedImageData =
                imageEncoder.EncodeImage(originalImageData, this.OriginalImage.PixelWidth, 
                    secretImageData, this.SecretImage.PixelWidth, this.SecretImage.PixelHeight);

            modifiedImageData = HeaderUtilities.AddHeader(modifiedImageData, this.OriginalImage.PixelWidth, false, 1,
                MessageType.MonochromeBmp);

            this.ModifiedImage = new WriteableBitmap(this.OriginalImage.PixelWidth, this.OriginalImage.PixelHeight);

            using var writeStream = this.ModifiedImage.PixelBuffer.AsStream();
            await writeStream.WriteAsync(modifiedImageData, 0, modifiedImageData.Length);
        }

        /// <summary>
        ///     Extracts the secret image from a ModifiedImage.
        /// </summary>
        public async Task ExtractSecretImage()
        {
            var modifiedImageData = await this.getImageData(this.ModifiedImage);

            if (HeaderUtilities.IsMessageEmbedded(modifiedImageData, this.ModifiedImage.PixelWidth) 
                && HeaderUtilities.GetMessageType(modifiedImageData, this.ModifiedImage.PixelWidth) == MessageType.MonochromeBmp)
            {
                var imageEncoder = new ImageEncoder();

                var secretImageData =
                    imageEncoder.DecodeImage(modifiedImageData, this.ModifiedImage.PixelWidth, this.ModifiedImage.PixelHeight);

                this.SecretImage = new WriteableBitmap(this.ModifiedImage.PixelWidth, this.ModifiedImage.PixelHeight);

                using var writeStream = this.SecretImage.PixelBuffer.AsStream();
                await writeStream.WriteAsync(secretImageData, 0, secretImageData.Length);
            } 
            else
            {
                throw new ArgumentException("Modified Image doesn't contain Secret Image");
            }
        }

        /// <summary>
        ///     Embeds the secret text message.
        /// </summary>
        public async Task EmbedSecretMessage()
        {
            var originalImageData = await this.getImageData(this.OriginalImage);

            var textEncoder = new TextEncoder();

            var modifiedImageData = textEncoder.EncodeMessage(originalImageData, this.SecretMessage);

            modifiedImageData = HeaderUtilities.AddHeader(modifiedImageData, this.OriginalImage.PixelWidth, false, 1,
                MessageType.MonochromeBmp);

            this.ModifiedImage = new WriteableBitmap(this.OriginalImage.PixelWidth, this.OriginalImage.PixelHeight);

            using var writeStream = this.ModifiedImage.PixelBuffer.AsStream();
            await writeStream.WriteAsync(modifiedImageData, 0, modifiedImageData.Length);
        }

        /// <summary>
        ///     Extracts the secret text message.
        /// </summary>
        public async Task ExtractSecretMessage()
        {
            var modifiedImageData = await this.getImageData(this.ModifiedImage);
            if (HeaderUtilities.IsMessageEmbedded(modifiedImageData, this.ModifiedImage.PixelWidth)
                && HeaderUtilities.GetMessageType(modifiedImageData, this.ModifiedImage.PixelWidth) == MessageType.Text)
            {
                var textEncoder = new TextEncoder();

                this.SecretMessage = textEncoder.DecodeMessage(modifiedImageData);
            }
            else
            {
                throw new ArgumentException("Modified Image doesn't contain Secret Text Message");
            }
        }

        /// <summary>
        ///     Gets the type of the secret message.
        /// </summary>
        /// <returns></returns>
        public async Task<MessageType> GetSecretType()
        {
            return HeaderUtilities.GetMessageType(await this.getImageData(this.ModifiedImage), this.ModifiedImage.PixelWidth);
        }

        /// <summary>
        ///     Encrypts the modified image.
        /// </summary>
        public async Task EncryptModifiedImage()
        {
            if (this.ModifiedImage == null)
            {
                return;
            }

            var encryptedModifiedImageData = ImageEncoder.EncryptImageData(await this.getImageData(this.ModifiedImage),
                this.ModifiedImage.PixelWidth);

            this.EncryptedModifiedImage = new WriteableBitmap(this.ModifiedImage.PixelWidth, this.ModifiedImage.PixelHeight);

            using var writeStream = this.EncryptedModifiedImage.PixelBuffer.AsStream();
            await writeStream.WriteAsync(encryptedModifiedImageData, 0, encryptedModifiedImageData.Length);
        }

        /// <summary>
        /// Saves the image.
        /// </summary>
        /// <param name="saveFile">The save file.</param>
        /// <param name="saveEncrypted">Save the modified Image as encrypted if true</param>
        /// <exception cref="ArgumentException">Invalid SaveFile.</exception>
        public async Task SaveImage(StorageFile saveFile, bool saveEncrypted)
        {
            if (saveFile == null)
            {
                throw new ArgumentException("Invalid Save File.");
            }

            var imageToSave = this.OriginalImage == null 
                ? this.SecretImage 
                : saveEncrypted 
                    ? this.EncryptedModifiedImage 
                    : this.ModifiedImage;

            var stream = await saveFile.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

            var pixelStream = imageToSave.PixelBuffer.AsStream();
            var pixels = new byte[pixelStream.Length];
            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                (uint) imageToSave.PixelWidth,
                (uint)imageToSave.PixelHeight, imageToSave.PixelHeight, imageToSave.PixelWidth, pixels);
            await encoder.FlushAsync();

            stream.Dispose();
        }

        /// <summary>
        ///     Saves the secret message to a text file.
        /// </summary>
        /// <param name="saveFile">The save file.</param>
        public async Task SaveSecretMessageToTextFile(StorageFile saveFile)
        {
            await FileIO.WriteTextAsync(saveFile, this.SecretMessage);
        }

        private static async Task<BitmapImage> makeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        private async Task<byte[]> getImageData(WriteableBitmap bitmap)
        {
            using var stream = bitmap.PixelBuffer.AsStream();
            var imageData = new byte[(uint) stream.Length];
            await stream.ReadAsync(imageData, 0, imageData.Length);

            return imageData;
        }

    }
}
