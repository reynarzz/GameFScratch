using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        private string RootAssetsFolderPath { get; set; } = "D:/Projects/Zkate";
        private const string AssetsFolderName = "Assets";

        public MainWindow()
        {
            InitializeComponent();
            FileListView.ItemsSource = FileItems;

            // File list drag/drop & selection helpers
            FileListView.PreviewMouseLeftButtonDown += FileListView_PreviewMouseLeftButtonDown;
            FileListView.MouseMove += FileListView_MouseMove;
            FileListView.AllowDrop = true;

            // Ensure right-click selects an item in the file list before showing the context menu
            FileListView.PreviewMouseRightButtonDown += FileListView_PreviewMouseRightButtonDown;
            FileListView.MouseRightButtonUp += FileListView_MouseRightButtonUp;

            // Folder tree drag/drop & selection helpers
            FolderTreeView.PreviewMouseRightButtonDown += FolderTreeView_PreviewMouseRightButtonDown;
            FolderTreeView.Drop += FolderTreeView_Drop;
            FolderTreeView.MouseRightButtonUp += FolderTreeView_MouseRightButtonUp;
            FolderTreeView.AllowDrop = true;

            this.Activated += MainWindow_Activated;
            this.Deactivated += MainWindow_Deactivated;
            FileListView.MouseDoubleClick += FileListView_MouseDoubleClick;

            var buildGesture = new KeyGesture(Key.B, ModifierKeys.Control);
            var buildCommand = new RoutedCommand();
            CommandBindings.Add(new CommandBinding(buildCommand, BuildButton_Click));
            InputBindings.Add(new KeyBinding(buildCommand, buildGesture));

            var settingsGesture = new KeyGesture(Key.G, ModifierKeys.Control);
            var settingsCommand = new RoutedCommand();
            CommandBindings.Add(new CommandBinding(settingsCommand, BuildConfigButton_Click));
            InputBindings.Add(new KeyBinding(settingsCommand, settingsGesture));

            RefreshFolderTree();
        }

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
        }

        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            RefreshFileList();
            RefreshFolderTree();
        }

        #region Folder Tree

        private void RefreshFolderTree()
        {
            // Remember expanded folders
            var expandedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            GetExpandedFolders(FolderTreeView.Items, expandedPaths);

            // Remember selected folder path (if any)
            string selectedPath = (FolderTreeView.SelectedItem as TreeViewItem)?.Tag as string;

            FolderTreeView.Items.Clear();
            
            var rootFolder = CreateTreeViewItem(RootAssetsFolderPath, AssetsFolderName);
            rootFolder.IsExpanded = true;
            
            FolderTreeView.Items.Add(rootFolder);
            RestoreExpandedState(rootFolder, expandedPaths);

            // Restore selection (try to select previous selected path)
            if (!string.IsNullOrEmpty(selectedPath))
            {
                SelectTreeViewItemByPath(FolderTreeView.Items, selectedPath);
            }
        }
        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Find the item under the mouse
            var lvItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (lvItem == null) return;

            if (lvItem.Content is not FileItem item) return;

            // Only handle folders
            if (!Directory.Exists(item.FullPath)) return;

            // Select the folder in the TreeView
            //if (SelectTreeViewItemByPath(FolderTreeView.Items, item.FullPath))
            //{
            //    // Refresh file list for the newly selected folder
            //    RefreshFileList();
            //}
        }

        private void GetExpandedFolders(ItemCollection items, HashSet<string> expandedPaths)
        {
            foreach (var obj in items)
            {
                if (obj is not TreeViewItem item) continue; // skip placeholders/nulls
                if (item.IsExpanded && item.Tag is string p && !string.IsNullOrEmpty(p))
                    expandedPaths.Add(p);

                if (item.Items != null && item.Items.Count > 0)
                    GetExpandedFolders(item.Items, expandedPaths);
            }
        }

        private void RestoreExpandedState(TreeViewItem item, HashSet<string> expandedPaths)
        {
            if (item == null || item.Tag is not string path) return;

            if (expandedPaths.Contains(path))
            {
                // expand and populate children
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
                catch {  }
            }
        }

        private bool SelectTreeViewItemByPath(ItemCollection items, string path)
        {
            foreach (var obj in items)
            {
                if (obj is not TreeViewItem item) continue;
                if (item.Tag as string == path)
                {
                    item.IsSelected = true;
                    item.BringIntoView();
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
                Tag = path,
                
            };
            if (Directory.EnumerateDirectories(path).Any())
            {
                // Only add dummy child if there are subfolders
                item.Items.Add(null);
                item.Expanded += Folder_Expanded; // lazy load
            }
            // Drag & drop events for folder items
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
                catch { /* ignore access exceptions */ }
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
            public bool IsFolder { get; set; }
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

        // Ensure right-click selects the item under the cursor before menu appears
        private void FileListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var lvItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (lvItem != null)
            {
                lvItem.IsSelected = true;
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

        #region Right Click Menus (with Rename)

        private void FileListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FileListView.SelectedItem is not FileItem item) return;

            ContextMenu menu = new ContextMenu();

            MenuItem rename = new MenuItem { Header = "Rename" };
            rename.Click += (_, _) =>
            {
                if (item.IsFolder)
                {
                    RenameFolder(item.FullPath);
                }
                else
                {
                    RenameFile(item);
                }
            };

            MenuItem delete = new MenuItem { Header = "Delete" };
            delete.Click += (_, _) =>
            {
                try
                {
                    if (item.IsFolder)
                    {

                        FileSystem.DeleteDirectory(item.FullPath,
                                   UIOption.OnlyErrorDialogs,
                                   RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        FileSystem.DeleteFile(item.FullPath,
                             UIOption.OnlyErrorDialogs,
                             RecycleOption.SendToRecycleBin);
                    }

                    RefreshFolderTree();
                    RefreshFileList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting file: {ex.Message}");
                }
            };

            menu.Items.Add(rename);
            menu.Items.Add(new Separator());
            menu.Items.Add(delete);
            menu.IsOpen = true;
        }

        private void FolderTreeView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FolderTreeView.SelectedItem is not TreeViewItem item) return;

            ContextMenu menu = new ContextMenu();

            MenuItem rename = new MenuItem { Header = "Rename Folder" };
            rename.Click += (_, _) =>
            {
                RenameFolder(item.Tag as string);
            };

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

            menu.Items.Add(rename);
            menu.Items.Add(new Separator());
            menu.Items.Add(delete);
            menu.IsOpen = true;
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

        #region Rename Implementations

        private void RenameFile(FileItem item)
        {
            string oldFullPath = item.FullPath;
            string oldName = item.Name;
            string dir = Path.GetDirectoryName(oldFullPath);
            if (string.IsNullOrEmpty(dir))
            {
                MessageBox.Show("Cannot determine file directory.");
                return;
            }

            string newName = PromptForInput("Rename file", oldName);
            if (string.IsNullOrEmpty(newName) || newName == oldName) return;

            if (!IsValidFileName(newName))
            {
                MessageBox.Show("Invalid file name.");
                return;
            }

            string newFullPath = Path.Combine(dir, newName);
            if (File.Exists(newFullPath) || Directory.Exists(newFullPath))
            {
                MessageBox.Show("A file or folder with that name already exists.");
                return;
            }

            try
            {
                File.Move(oldFullPath, newFullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to rename file: {ex.Message}");
                return;
            }

            // Refresh and select renamed file
            RefreshFolderTree();
            RefreshFileList();

            var renamed = FileItems.FirstOrDefault(f => string.Equals(f.FullPath, newFullPath, StringComparison.OrdinalIgnoreCase));
            if (renamed != null)
                FileListView.SelectedItem = renamed;
        }

        private void RenameFolder(string oldPath)
        {
            if (string.IsNullOrEmpty(oldPath))
            {
                MessageBox.Show("Cannot determine folder path.");
                return;
            }

            // Do not allow renaming drive roots
            string root = Path.GetPathRoot(oldPath);
            if (!string.IsNullOrEmpty(root) && string.Equals(root.TrimEnd('\\', '/'), oldPath.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Cannot rename a drive root.");
                return;
            }

            string parentDir = Path.GetDirectoryName(oldPath);
            if (string.IsNullOrEmpty(parentDir))
            {
                MessageBox.Show("Cannot determine parent directory.");
                return;
            }

            string oldName = Path.GetFileName(oldPath);
            string newName = PromptForInput("Rename folder", oldName);
            if (string.IsNullOrEmpty(newName) || newName == oldName) return;

            if (!IsValidFileName(newName))
            {
                MessageBox.Show("Invalid folder name.");
                return;
            }

            string newPath = Path.Combine(parentDir, newName);
            if (Directory.Exists(newPath) || File.Exists(newPath))
            {
                MessageBox.Show("A file or folder with that name already exists.");
                return;
            }

            try
            {
                Directory.Move(oldPath, newPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to rename folder: {ex.Message}");
                return;
            }

            // Refresh tree and select the renamed folder
            RefreshFolderTree();
            SelectTreeViewItemByPath(FolderTreeView.Items, newPath);
            RefreshFileList();
        }

        #endregion

        #region Helpers & Refresh

        private static bool IsValidFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            char[] invalid = Path.GetInvalidFileNameChars();
            return name.IndexOfAny(invalid) < 0;
        }

        // Prompt small dialog to get new name (returns null if canceled)
        private string PromptForInput(string title, string defaultValue)
        {
            Window dialog = new Window
            {
                Title = title,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 360,
                Height = 130,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                WindowStyle = WindowStyle.ToolWindow,
                ShowInTaskbar = false
            };

            var panel = new StackPanel { Margin = new Thickness(10) };
            var tbLabel = new TextBlock { Text = "New name:", Margin = new Thickness(0, 0, 0, 6) };
            var textBox = new TextBox { Text = defaultValue ?? "", Margin = new Thickness(0, 0, 0, 10) };
            textBox.SelectAll();

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "OK", Width = 80, IsDefault = true, Margin = new Thickness(0, 0, 6, 0) };
            var cancelButton = new Button { Content = "Cancel", Width = 80, IsCancel = true };

            okButton.Click += (_, _) => { dialog.DialogResult = true; dialog.Close(); };
            cancelButton.Click += (_, _) => { dialog.DialogResult = false; dialog.Close(); };

            buttons.Children.Add(okButton);
            buttons.Children.Add(cancelButton);

            panel.Children.Add(tbLabel);
            panel.Children.Add(textBox);
            panel.Children.Add(buttons);

            dialog.Content = panel;

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                return textBox.Text.Trim();
            }
            return null;
        }

        private void RefreshFileList()
        {
            FileItems.Clear();

            if (FolderTreeView.SelectedItem is not TreeViewItem selectedItem)
            {
                CurrentPathLabel.Text = string.Empty;
                return;
            }

            string path = selectedItem.Tag as string;
            CurrentPathLabel.Text = path.Replace(RootAssetsFolderPath, AssetsFolderName) ?? string.Empty;

            if (string.IsNullOrEmpty(path)) return;

            try
            {
                // Add folders first
                foreach (var dir in Directory.GetDirectories(path))
                {
                    DirectoryInfo info = new DirectoryInfo(dir);
                    FileItems.Add(new FileItem
                    {
                        Name = info.Name,
                        Type = "Folder",
                        Size = "",
                        FullPath = info.FullName,
                        IsFolder = true
                    });
                }

                // Then add files
                foreach (var file in Directory.GetFiles(path).Where(x => !x.EndsWith(".meta")))
                {
                    FileInfo info = new FileInfo(file);
                    FileItems.Add(new FileItem
                    {
                        Name = info.Name,
                        Type = !string.IsNullOrEmpty(info.Extension)? info.Extension: "-",
                        Size = $"{info.Length / 1024} KB",
                        FullPath = info.FullName,
                        IsFolder = false
                    });
                }
            }
            catch
            {
            }
        }

        // Generic VisualTreeWalker helper
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
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
            var buildWindow = new BuildWindow
            {
                Owner = this
            };

            // Start the build asynchronously BEFORE showing the window
            _ = Task.Run(async () =>
            {
                string[] filesToBuild = new string[]
                {
            "file1.txt",
            "file2.png",
            "file3.obj",
            "file4.shader"
                };

                for (int i = 0; i < filesToBuild.Length; i++)
                {
                    string file = filesToBuild[i];

                    await Task.Delay(500); // Simulate work

                    // Update UI safely
                    buildWindow.Dispatcher.Invoke(() =>
                    {
                        double percent = (i + 1) * 100.0 / filesToBuild.Length;
                        buildWindow.CurrentFileLabel.Text = $"Current File: {file}";
                        buildWindow.CurrentWorkLabel.Text = "Processing...";
                        buildWindow.BuildProgressBar.Value = percent;
                        buildWindow.PercentageLabel.Text = $"{percent:0}%";
                    });

                    // Stop if cancel requested
                    if (buildWindow.CancelRequested)
                        break;
                }

                buildWindow.Dispatcher.Invoke(() =>
                {
                    buildWindow.CurrentWorkLabel.Text = buildWindow.CancelRequested ? "Build Canceled" : "Build Complete!";
                    buildWindow.CurrentFileLabel.Text = "";
                });
            });

            // Show the window modally
            buildWindow.ShowDialog();
        }


        #endregion
    }
}
