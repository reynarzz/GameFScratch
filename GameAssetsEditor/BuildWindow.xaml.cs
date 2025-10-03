using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace GameAssetsEditor
{
    public partial class BuildWindow : Window
    {
        public bool CancelRequested { get; private set; } = false;

        public BuildWindow()
        {
            InitializeComponent();
            Loaded += BuildWindow_Loaded;
        }

        private void BuildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RemoveCloseButton();
        }

        public void UpdateProgress(string currentWork, string currentFile, double progressPercent)
        {
            CurrentWorkLabel.Text = currentWork;
            CurrentFileLabel.Text = $"Current File: {currentFile}";
            BuildProgressBar.Value = progressPercent;
            PercentageLabel.Text = $"{progressPercent:0}%";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            CancelRequested = true;
        }

        // Disable the close button
        private void RemoveCloseButton()
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_SYSMENU);
        }

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
