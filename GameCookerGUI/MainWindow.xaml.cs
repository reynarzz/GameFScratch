using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GameAssetsEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Allowed file extensions
        private readonly string[] imageExtensions = { ".png", ".jpg", ".jpeg" };
        private readonly string[] audioExtensions = { ".mp3", ".wav" };
        private readonly string[] textExtensions = { ".txt", ".json" };
        private bool compressFiles = false;
        private bool encryptImages = false;
        private bool encryptAudio = false;
        private bool encryptText = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadFolderTree();
        }

        private void LoadFolderTree()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                TreeViewItem driveItem = new TreeViewItem
                {
                    Header = drive.Name,
                    Tag = drive.RootDirectory.FullName
                };
                driveItem.Expanded += Folder_Expanded;
                driveItem.Items.Add(null); // Placeholder
                FolderTreeView.Items.Add(driveItem);
            }
        }

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();
                string path = (string)item.Tag;
                try
                {
                    foreach (var dir in Directory.GetDirectories(path))
                    {
                        TreeViewItem subItem = new TreeViewItem
                        {
                            Header = System.IO.Path.GetFileName(dir),
                            Tag = dir
                        };
                        subItem.Items.Add(null);
                        subItem.Expanded += Folder_Expanded;
                        item.Items.Add(subItem);
                    }
                }
                catch {  }
            }
        }

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FileListView.Items.Clear();
            if (FolderTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                string path = (string)selectedItem.Tag;
                try
                {
                    var files = Directory.GetFiles(path)
                        .Where(f => imageExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()) ||
                                    audioExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()) ||
                                    textExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()));

                    foreach (var file in files)
                    {
                        FileInfo info = new FileInfo(file);
                        FileListView.Items.Add(new
                        {
                            Name = info.Name,
                            Type = info.Extension,
                            Size = $"{info.Length / 1024} KB"
                        });
                    }
                }
                catch {  }
            }
        }

     

        private void BuildConfigButton_Click(object sender, RoutedEventArgs e)
        {
            BuildConfigurationWindow configWindow = new BuildConfigurationWindow
            {
                Owner = this
            };

            if (configWindow.ShowDialog() == true)
            {
                compressFiles = configWindow.Compress;
                encryptImages = configWindow.EncryptImages;
                encryptAudio = configWindow.EncryptAudio;
                encryptText = configWindow.EncryptText;

                MessageBox.Show($"Configuration saved:\nCompress: {compressFiles}\nEncrypt Images: {encryptImages}\nEncrypt Audio: {encryptAudio}\nEncrypt Text: {encryptText}");
            }
        }

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Build started with settings:\nCompress: {compressFiles}\nEncrypt Images: {encryptImages}\nEncrypt Audio: {encryptAudio}\nEncrypt Text: {encryptText}");
        }
    }
}
