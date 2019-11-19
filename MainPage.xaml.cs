using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using GroupHStegafy.Controllers;

namespace GroupHStegafy
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private static readonly double ApplicationHeight = (double)Application.Current.Resources["AppHeight"];
        private static readonly double ApplicationWidth = (double)Application.Current.Resources["AppWidth"];

        private readonly ImageManager imageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size { Width = ApplicationWidth, Height = ApplicationHeight };
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView()
                           .SetPreferredMinSize(new Size(ApplicationWidth, ApplicationHeight));

            this.imageManager = new ImageManager();
        }

        private async void openOriginalImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.imageManager.ModifiedImage != null)
            {
                return;
            }

            var sourceImageFile = await selectSourceImageFile();
            if (sourceImageFile == null)
            {
                return;
            }

            await this.imageManager.ReadOriginalImage(sourceImageFile);
            this.originalImageDisplay.Source = this.imageManager.OriginalImage;

            if (this.imageManager.SecretImage != null)
            {
                await this.imageManager.EmbedSecretImage();
                this.modifiedImageDisplay.Source = this.imageManager.ModifiedImage;
            }

            this.openSecretImageButton.IsEnabled = true;
            this.openModifiedImageButton.IsEnabled = false;
        }

        private async void openModifiedImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.imageManager.OriginalImage != null)
            {
                return;
            }

            var sourceImageFile = await selectSourceImageFile();
            if (sourceImageFile == null)
            {
                return;
            }

            await this.imageManager.ReadModifiedImage(sourceImageFile);
            this.modifiedImageDisplay.Source = this.imageManager.ModifiedImage;
            await this.imageManager.ExtractSecretImage();
            this.secretImageDisplay.Source = this.imageManager.SecretImage;

            this.openOriginalImageButton.IsEnabled = false;
            this.saveButton.IsEnabled = true;
        }

        private async void openSecretImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.imageManager.OriginalImage == null)
            {
                return;
            }

            var sourceImageFile = await selectSourceImageFile();
            if (sourceImageFile == null)
            {
                return;
            }

            await this.imageManager.ReadSecretImage(sourceImageFile);
            this.secretImageDisplay.Source = this.imageManager.SecretImage;
            await this.imageManager.EmbedSecretImage();
            this.modifiedImageDisplay.Source = this.imageManager.ModifiedImage;

            this.saveButton.IsEnabled = true;
        }

        private static async Task<StorageFile> selectSourceImageFile()
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            var file = await openPicker.PickSingleFileAsync();

            return file;
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.imageManager.ModifiedImage == null)
            {
                return;
            }

            var saveFile = await selectSaveFile();
            if (saveFile != null)
            {
                await this.imageManager.SaveImage(saveFile);
            }
        }

        private static async Task<StorageFile> selectSaveFile()
        {
            var fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "image"
            };
            fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> { ".png" });

            var saveFile = await fileSavePicker.PickSaveFileAsync();

            return saveFile;
        }
    }
}
