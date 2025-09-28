using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace GameAssetsEditor
{

    public partial class BuildConfigurationWindow : Window
    {
        public bool Compress { get; private set; }
        public bool EncryptImages { get; private set; }
        public bool EncryptAudio { get; private set; }
        public bool EncryptText { get; private set; }
        public bool CompressImages { get; private set; }
        public bool CompressAudio { get; private set; }
        public bool CompressText { get; private set; }

        private readonly string[] imageExtensions = { ".png", ".jpg", ".jpeg" };
        private readonly string[] audioExtensions = { ".mp3", ".wav" };
        private readonly string[] textExtensions = { ".txt", ".json" };

        public BuildConfigurationWindow(string folderPath = null)
        {
            InitializeComponent();

            // Attach events AFTER controls are created
            CompressCheckBox.Checked += CompressCheckBox_Checked;
            CompressCheckBox.Unchecked += CompressCheckBox_Checked;

            EnableEncryptionCheckBox.Checked += EnableEncryptionCheckBox_Checked;
            EnableEncryptionCheckBox.Unchecked += EnableEncryptionCheckBox_Checked;

            CompressCheckBox.IsChecked = true;
            EnableEncryptionCheckBox.IsChecked = true;

            // Initialize enabled/disabled states based on IsChecked
            CompressCheckBox_Checked(null, null);
            EnableEncryptionCheckBox_Checked(null, null);

            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                PreCheckFileTypes(folderPath);
            }
        }

        private void PreCheckFileTypes(string folderPath)
        {
            try
            {
                var files = Directory.GetFiles(folderPath);

                if (files.Any(f => imageExtensions.Contains(Path.GetExtension(f).ToLower())))
                    EncryptImagesCheckBox.IsChecked = true;

                if (files.Any(f => audioExtensions.Contains(Path.GetExtension(f).ToLower())))
                    EncryptAudioCheckBox.IsChecked = true;

                if (files.Any(f => textExtensions.Contains(Path.GetExtension(f).ToLower())))
                    EncryptTextCheckBox.IsChecked = true;
            }
            catch
            {
                // Ignore access exceptions
            }
        }

        
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Compress = CompressCheckBox.IsChecked == true;
            CompressImages = CompressImagesCheckBox.IsChecked == true;
            CompressAudio = CompressAudioCheckBox.IsChecked == true;
            CompressText = CompressTextCheckBox.IsChecked == true;

            EncryptImages = EncryptImagesCheckBox.IsChecked == true;
            EncryptAudio = EncryptAudioCheckBox.IsChecked == true;
            EncryptText = EncryptTextCheckBox.IsChecked == true;

            DialogResult = true;
            Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void EnableEncryptionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool enabled = EnableEncryptionCheckBox.IsChecked == true;
            EncryptImagesCheckBox.IsEnabled = enabled;
            EncryptAudioCheckBox.IsEnabled = enabled;
            EncryptTextCheckBox.IsEnabled = enabled;
        }

        private void CompressCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool enabled = CompressCheckBox.IsChecked == true;
            CompressImagesCheckBox.IsEnabled = enabled;
            CompressAudioCheckBox.IsEnabled = enabled;
            CompressTextCheckBox.IsEnabled = enabled;
        }
    }
}
