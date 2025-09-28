using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GameAssetsEditor
{
    public partial class MainWindow : Window
    {
        private readonly string[] imageExtensions = { ".png", ".jpg", ".jpeg" };
        private readonly string[] audioExtensions = { ".mp3", ".wav" };
        private readonly string[] textExtensions = { ".txt", ".json" };

        private bool compressFiles = false;
        private bool encryptImages = false;
        private bool encryptAudio = false;
        private bool encryptText = false;

        public ObservableCollection<FileItem> FileItems { get; set; } = new ObservableCollection<FileItem>();
        private Point _fileStartPoint;
        private Point _folderStartPoint;

        public MainWindow()
        {
            InitializeComponent();
            FileListView.ItemsSource = FileItems;

            FileListView.PreviewMouseLeftButtonDown += FileListView_PreviewMouseLeftButtonDown;
            FileListView.MouseMove += FileListView_MouseMove;
            FileListView.AllowDrop = true;
            FileListView.MouseRightButtonUp += FileListView_MouseRightButtonUp;

            FolderTreeView.PreviewMouseRightButtonDown += FolderTreeView_PreviewMouseRightButtonDown;
            FolderTreeView.Drop += FolderTreeView_Drop;
            FolderTreeView.MouseRightButtonUp += FolderTreeView_MouseRightButtonUp;
            FolderTreeView.AllowDrop = true;

            RefreshFolderTree();
        }

        #region Folder Tree

        private void RefreshFolderTree()
        {
            // Remember expanded folders
            var expandedPaths = new HashSet<string>();
            GetExpandedFolders(FolderTreeView.Items, expandedPaths);

            // Remember selected folder
            string selectedPath = (FolderTreeView.SelectedItem as TreeViewItem)?.Tag as string;

            // Reload drives
            FolderTreeView.Items.Clear();
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                var driveItem = CreateTreeViewItem(drive.RootDirectory.FullName, drive.Name);
                FolderTreeView.Items.Add(driveItem);
                RestoreExpandedState(driveItem, expandedPaths);
            }

            // Restore selection
            if (!string.IsNullOrEmpty(selectedPath))
            {
                SelectTreeViewItemByPath(FolderTreeView.Items, selectedPath);
            }
        }

        private void GetExpandedFolders(ItemCollection items, HashSet<string> expandedPaths)
        {
            foreach (var obj in items)
            {
                if (obj is not TreeViewItem item) continue; // skip nulls
                if (item.IsExpanded && item.Tag is string path)
                    expandedPaths.Add(path);

                if (item.Items != null && item.Items.Count > 0)
                    GetExpandedFolders(item.Items, expandedPaths);
            }
        }

        private void RestoreExpandedState(TreeViewItem item, HashSet<string> expandedPaths)
        {
            if (item == null || item.Tag is not string path) return;

            if (expandedPaths.Contains(path))
            {
                item.IsExpanded = true;
                item.Items.Clear();
                try
                {
                    foreach (var dir in Directory.GetDirectories(path))
                    {
                        var subItem = CreateTreeViewItem(dir, System.IO.Path.GetFileName(dir));
                        item.Items.Add(subItem);
                        RestoreExpandedState(subItem, expandedPaths);
                    }
                }
                catch { }
            }
        }


        private bool SelectTreeViewItemByPath(ItemCollection items, string path)
        {
            foreach (var obj in items)
            {
                if (obj is not TreeViewItem item) continue; // skip nulls and non-TreeViewItems
                if (item.Tag as string == path)
                {
                    item.IsSelected = true;
                    return true;
                }
                if (SelectTreeViewItemByPath(item.Items, path))
                    return true;
            }
            return false;
        }

        private TreeViewItem CreateTreeViewItem(string path, string header)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = header,
                Tag = path
            };
            item.Items.Add(null);
            item.Expanded += Folder_Expanded;

            // Drag & drop events
            item.PreviewMouseLeftButtonDown += TreeViewItem_PreviewMouseLeftButtonDown;
            item.MouseMove += TreeViewItem_MouseMove;
            item.AllowDrop = true;
            item.DragEnter += TreeViewItem_DragEnter;
            item.DragLeave += TreeViewItem_DragLeave;
            item.Drop += TreeViewItem_Drop;

            return item;
        }

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not TreeViewItem item) return;
            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();
                string path = item.Tag as string;
                try
                {
                    foreach (var dir in Directory.GetDirectories(path))
                    {
                        var subItem = CreateTreeViewItem(dir, System.IO.Path.GetFileName(dir));
                        item.Items.Add(subItem);
                    }
                }
                catch { }
            }
        }

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RefreshFileList();
        }

        #endregion

        #region FileItem Class

        public class FileItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Size { get; set; }
            public string FullPath { get; set; }
        }

        #endregion

        #region FileListView Drag & Drop

        private void FileListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _fileStartPoint = e.GetPosition(null);
        }

        private void FileListView_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _fileStartPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (FileListView.SelectedItems.Count == 0) return;

                string[] paths = FileListView.SelectedItems.Cast<FileItem>().Select(f => f.FullPath).ToArray();
                DataObject data = new DataObject(DataFormats.FileDrop, paths);
                DragDrop.DoDragDrop(FileListView, data, DragDropEffects.Move);
            }
        }

        #endregion

        #region Folder Drag & Drop

        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _folderStartPoint = e.GetPosition(null);
        }

        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _folderStartPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (sender is TreeViewItem item)
                {
                    string folderPath = item.Tag as string;
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        DataObject data = new DataObject(DataFormats.FileDrop, new string[] { folderPath });
                        DragDrop.DoDragDrop(item, data, DragDropEffects.Move);
                    }
                }
            }
        }

        private void TreeViewItem_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is TreeViewItem item)
                item.Background = Brushes.LightBlue;
        }

        private void TreeViewItem_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is TreeViewItem item)
                item.ClearValue(TreeViewItem.BackgroundProperty);
        }

        private void TreeViewItem_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            if (sender is not TreeViewItem targetItem) return;

            string targetFolder = targetItem.Tag as string;
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var src in paths)
            {
                string dest = System.IO.Path.Combine(targetFolder, System.IO.Path.GetFileName(src));
                try
                {
                    if (Directory.Exists(src))
                        Directory.Move(src, dest);
                    else if (File.Exists(src))
                        File.Move(src, dest);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error moving '{src}': {ex.Message}");
                }
            }

            targetItem.ClearValue(TreeViewItem.BackgroundProperty);

            RefreshFolderTree();
            RefreshFileList();
        }

        private void FolderTreeView_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            if (FolderTreeView.SelectedItem is not TreeViewItem targetItem) return;

            TreeViewItem_Drop(targetItem, e);
        }

        #endregion

        #region Right Click Menus

        private void FileListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FileListView.SelectedItem is FileItem item)
            {
                ContextMenu menu = new ContextMenu();
                MenuItem delete = new MenuItem { Header = "Delete" };
                delete.Click += (_, _) =>
                {
                    try
                    {
                        File.Delete(item.FullPath);
                        RefreshFolderTree();
                        RefreshFileList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting file: {ex.Message}");
                    }
                };
                menu.Items.Add(delete);
                menu.IsOpen = true;
            }
        }

        private void FolderTreeView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FolderTreeView.SelectedItem is TreeViewItem item)
            {
                ContextMenu menu = new ContextMenu();
                MenuItem delete = new MenuItem { Header = "Delete Folder" };
                delete.Click += (_, _) =>
                {
                    try
                    {
                        Directory.Delete(item.Tag as string, true);
                        RefreshFolderTree();
                        RefreshFileList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting folder: {ex.Message}");
                    }
                };
                menu.Items.Add(delete);
                menu.IsOpen = true;
            }
        }

        private void FolderTreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;
            while (obj != null && !(obj is TreeViewItem))
                obj = VisualTreeHelper.GetParent(obj);

            if (obj is TreeViewItem item)
            {
                item.IsSelected = true;
                e.Handled = true;
            }
        }

        #endregion

        #region Refresh Helpers

        private void RefreshFileList()
        {
            FileItems.Clear();
            if (FolderTreeView.SelectedItem is not TreeViewItem selectedItem) return;

            string path = selectedItem.Tag as string;
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var files = Directory.GetFiles(path)
                    .Where(f => imageExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()) ||
                                audioExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()) ||
                                textExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()));

                foreach (var file in files)
                {
                    FileInfo info = new FileInfo(file);
                    FileItems.Add(new FileItem
                    {
                        Name = info.Name,
                        Type = info.Extension,
                        Size = $"{info.Length / 1024} KB",
                        FullPath = info.FullName
                    });
                }
            }
            catch { }
        }

        #endregion

        #region Build Config & Build

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

        #endregion
    }
}
