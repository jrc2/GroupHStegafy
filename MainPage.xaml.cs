using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GroupHStegafy.Controllers;
using GroupHStegafy.Utilities;

namespace GroupHStegafy
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private static readonly double ApplicationHeight = (double)Application.Current.Resources["AppHeight"];
        private static readonly double ApplicationWidth = (double)Application.Current.Resources["AppWidth"];

        private readonly StegafyManager stegafyManager;

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

            this.stegafyManager = new StegafyManager();
        }

        private async void openOriginalImageButton_Click(object sender, RoutedEventArgs e)
        {
            this.clearErrorMessage();
            if (this.stegafyManager.ModifiedImage != null)
            {
                return;
            }

            var sourceImageFile = await selectSourceImageFile();
            if (sourceImageFile == null)
            {
                return;
            }

            await this.stegafyManager.ReadOriginalImage(sourceImageFile);
            this.originalImageDisplay.Source = this.stegafyManager.OriginalImage;

            if (this.stegafyManager.SecretImage != null)
            {
                await this.stegafyManager.EmbedSecretImage(false);
                this.modifiedImageDisplay.Source = this.stegafyManager.ModifiedImage;
            }

            this.openSecretFileButton.IsEnabled = true;
            this.encryptCheckbox.IsEnabled = true;
            this.unencryptedSecretMessageTextBlock.Visibility = Visibility.Visible;
            this.cipherWordTextBlock.Visibility = Visibility.Visible;
            this.cipherWordTextBox.Visibility = Visibility.Visible;
            this.bitsPerColorChannelTextBox.IsEnabled = true;
            this.embedSecretMessageButton.IsEnabled = true;
            this.openModifiedImageButton.IsEnabled = false;
        }

        private async void openModifiedImageButton_Click(object sender, RoutedEventArgs e)
        {
            this.clearErrorMessage();

            var sourceImageFile = await selectSourceImageFile();
            if (sourceImageFile == null)
            {
                return;
            }

            try
            {
                await this.stegafyManager.ReadModifiedImage(sourceImageFile);
                if (await this.stegafyManager.GetSecretType() == MessageType.MonochromeBmp)
                {
                    await this.extractSecretImage();
                }
                else
                {
                    await this.extractSecretMessage();
                }
            }
            catch (ArgumentException exception)
            {
                this.displayErrorMessage(exception.Message);
                return;
            }

            this.openOriginalImageButton.IsEnabled = false;
            this.saveButton.IsEnabled = true;
        }

        private async void openSecretFileButton_Click(object sender, RoutedEventArgs e)
        {
            this.clearErrorMessage();
            if (this.stegafyManager.OriginalImage == null)
            {
                return;
            }

            var sourceSecretFile = await selectSourceFile();
            if (sourceSecretFile == null)
            {
                return;
            }

            if (await HeaderUtilities.IsImageFile(sourceSecretFile))
            {
                await this.embedSecretImage(sourceSecretFile);
                if (this.stegafyManager.ModifiedImage == null)
                {
                    return;
                }
                this.unencryptedSecretImageDisplay.Visibility = Visibility.Visible;
                this.unencryptedSecretImageDisplay.Source = this.stegafyManager.SecretImage;
                this.modifiedImageDisplay.Source = this.stegafyManager.ModifiedImage;
                this.embedSecretMessageButton.IsEnabled = false;
            }
            else
            {
                await this.stegafyManager.ReadSecretMessageFromTextFile(sourceSecretFile);
                if (this.stegafyManager.SecretMessage == null)
                {
                    return;
                }
                this.unencryptedSecretMessageTextBlock.Visibility = Visibility.Visible;
                this.unencryptedSecretMessageTextBlock.Text = this.stegafyManager.SecretMessage;
                this.modifiedImageDisplay.Source = this.stegafyManager.ModifiedImage;
            }

            this.saveButton.IsEnabled = true;
            this.encryptCheckbox.IsEnabled = true;
        }

        private async void embedSecretMessageButton_Click(object sender, RoutedEventArgs e)
        {
            this.clearErrorMessage();
            if (this.unencryptedSecretMessageTextBlock.Text.Equals(""))
            {
                this.displayErrorMessage("Invalid Secret Message.");
                return;
            }

            var stripNonAlpha = new Regex("[^a-zA-Z]");
            this.stegafyManager.SecretMessage = stripNonAlpha.Replace(this.unencryptedSecretMessageTextBlock.Text, "");

            try
            {
                if (int.TryParse(this.bitsPerColorChannelTextBox.Text, out var bitsPerColorChannel))
                {
                    await this.stegafyManager.EmbedSecretMessage(bitsPerColorChannel, true);
                }
                else
                {
                    this.displayErrorMessage("Invalid Bits per Color Channel.");
                    return;
                }
            }
            catch (ArgumentException exception)
            {
                this.displayErrorMessage(exception.Message);
                return;
            }

            this.modifiedImageDisplay.Source = this.stegafyManager.ModifiedImage;

            this.saveButton.IsEnabled = true;
            this.encryptCheckbox.IsEnabled = true;
        }

        private async Task extractSecretImage()
        {
            this.modifiedImageDisplay.Source = this.stegafyManager.ModifiedImage;

            await this.stegafyManager.ExtractSecretImage();
            this.unencryptedSecretImageDisplay.Source = this.stegafyManager.SecretImage;
            this.unencryptedSecretImageDisplay.Visibility = Visibility.Visible;

            this.stegafyManager.SecretMessage = null;
            this.unencryptedSecretMessageTextBlock.Visibility = Visibility.Collapsed;
            this.encryptedSecretMessageTextBlock.Visibility = Visibility.Collapsed;

            if (await this.stegafyManager.IsModifiedImageSecretEncrypted())
            {
                this.encryptedSecretImageDisplay.Source = this.stegafyManager.EncryptedSecretImage;
                this.encryptedSecretImageDisplay.Visibility = Visibility.Visible;
            }
        }

        private async Task extractSecretMessage()
        {
            this.modifiedImageDisplay.Source = this.stegafyManager.ModifiedImage;

            this.encryptedSecretImageDisplay.Visibility = Visibility.Collapsed;
            this.unencryptedSecretImageDisplay.Visibility = Visibility.Collapsed;

            if (await this.stegafyManager.IsModifiedImageSecretEncrypted())
            {
                await this.stegafyManager.ExtractSecretMessage();
                this.encryptedSecretMessageTextBlock.Text = this.stegafyManager.SecretMessage;
                this.encryptedSecretMessageTextBlock.Visibility = Visibility.Visible;
                this.encryptedSecretMessageTextBlock.IsReadOnly = true;
                this.stegafyManager.DecryptSecretMessage();
                this.unencryptedSecretMessageTextBlock.Text = this.stegafyManager.SecretMessage;
                this.unencryptedSecretMessageTextBlock.Visibility = Visibility.Visible;
                this.unencryptedSecretMessageTextBlock.IsReadOnly = true;
                this.cipherWordTextBox.Visibility = Visibility.Visible;
                this.cipherWordTextBox.IsReadOnly = true;
                this.cipherWordTextBox.Text = TextUtilities.GetKey(this.stegafyManager.SecretMessage);
            }
            else
            {
                await this.stegafyManager.ExtractSecretMessage();
                this.unencryptedSecretMessageTextBlock.Text = this.stegafyManager.SecretMessage;
                this.unencryptedSecretMessageTextBlock.Visibility = Visibility.Visible;
                this.unencryptedSecretMessageTextBlock.IsReadOnly = true;
            }
        }

        private async Task embedSecretImage(StorageFile sourceImageFile)
        {
            await this.stegafyManager.ReadSecretImage(sourceImageFile);
            try
            {
                await this.stegafyManager.EmbedSecretImage(false);
            }
            catch (ArgumentException exception)
            {
                this.displayErrorMessage(exception.Message);
            }
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

        private static async Task<StorageFile> selectSourceFile()
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add("*");

            var file = await openPicker.PickSingleFileAsync();

            return file;
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            this.clearErrorMessage();
            if (this.stegafyManager.ModifiedImage == null)
            {
                return;
            }

            if (this.stegafyManager.IsSaveFileText())
            {
                var saveFile = await selectSaveTextFile();
                if (saveFile != null)
                {
                    await this.stegafyManager.SaveSecretMessageToTextFile(saveFile);
                }
            }
            else
            {
                var saveFile = await selectSaveImageFile();
                if (saveFile != null && this.encryptCheckbox.IsChecked.HasValue)
                {
                    await this.stegafyManager.SaveImage(saveFile);
                }
            }
            
        }

        private static async Task<StorageFile> selectSaveImageFile()
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

        private static async Task<StorageFile> selectSaveTextFile()
        {
            var fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "text"
            };
            fileSavePicker.FileTypeChoices.Add("Text files", new List<string> { ".txt" });

            var saveFile = await fileSavePicker.PickSaveFileAsync();

            return saveFile;
        }

        private void displayErrorMessage(string errorMessage)
        {
            this.errorTextBlock.Text = errorMessage;
        }

        private void clearErrorMessage()
        {
            this.errorTextBlock.Text = "";
        }

        private async void encryptCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            this.clearErrorMessage();

            if (this.stegafyManager.SecretMessage == null && this.stegafyManager.SecretImage == null)
            {
                this.displayErrorMessage("Invalid Secret Message");
                this.encryptCheckbox.IsChecked = false;
                return;
            }

            if (this.stegafyManager.SecretMessage == null)
            {
                await this.stegafyManager.EncryptSecretImage();
                await this.stegafyManager.EmbedSecretImage(true);
                this.encryptedSecretImageDisplay.Visibility = Visibility.Visible;
                this.encryptedSecretImageDisplay.Source = this.stegafyManager.EncryptedSecretImage;
            }
            else
            {
                try
                {
                    this.stegafyManager.EncryptSecretMessage(this.cipherWordTextBox.Text, this.unencryptedSecretMessageTextBlock.Text);
                    if (!int.TryParse(this.bitsPerColorChannelTextBox.Text, out var bitsPerColorChannel))
                    {
                        this.displayErrorMessage("Invalid Bits Per Color Channel");
                        return;
                    }

                    await this.stegafyManager.EmbedSecretMessage(bitsPerColorChannel, true);
                    this.cipherWordTextBox.IsReadOnly = true;
                }
                catch (ArgumentException exception)
                {
                    this.displayErrorMessage(exception.Message);
                    this.encryptCheckbox.IsChecked = false;
                    return;
                }
                this.encryptedSecretMessageTextBlock.Visibility = Visibility.Visible;
                this.encryptedSecretMessageTextBlock.Text = this.stegafyManager.SecretMessage;
            }
            
        }

        private async void encryptCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            this.clearErrorMessage();
            if (this.stegafyManager.SecretMessage == null)
            {
                await this.stegafyManager.EncryptSecretImage();
                this.encryptedSecretImageDisplay.Visibility = Visibility.Collapsed;
            }
            else
            {
                try
                {
                    this.stegafyManager.DecryptSecretMessage();
                    this.cipherWordTextBox.IsReadOnly = false;
                }
                catch (ArgumentException exception)
                {
                    this.displayErrorMessage(exception.Message);
                    return;
                }
                this.encryptedSecretMessageTextBlock.Visibility = Visibility.Collapsed;
            }
            
        }

        private async void reloadButton_Click(object sender, RoutedEventArgs e)
        {
            await CoreApplication.RequestRestartAsync("");
        }
    }
}
