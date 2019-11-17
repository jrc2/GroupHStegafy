using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using GroupHStegafy.Controllers;

namespace GroupHStegafy
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly ImageManager imageEditor;

        public MainPage()
        {
            this.InitializeComponent();

            this.imageEditor = new ImageManager();
        }

        private async void openButton_Click(object sender, RoutedEventArgs e)
        {
            var sourceImageFile = await selectSourceImageFile();
            if (sourceImageFile == null)
            {
                return;
            }

            await this.imageEditor.ReadImage(sourceImageFile);

            this.imageDisplay.Source = this.imageEditor.OriginalImage;
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
            if (this.imageEditor.ModifiedImage == null)
            {
                return;
            }

            var saveFile = await selectSaveFile();
            if (saveFile != null)
            {
                await this.imageEditor.SaveImage(saveFile);
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
