using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using EasySaveInterface.ViewModels;

namespace EasySaveInterface.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SelectFolder(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
                return;

            if (DataContext is not MainWindowViewModel vm)
                return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    Title = vm.TextBrowserTitle,
                    AllowMultiple = false
                });

            if (folders.Count > 0)
            {
                string selectedPath = folders[0].Path.LocalPath;
                string? tag = (sender as Button)?.Tag?.ToString();

                if (tag == "source")
                    vm.NewJobSource = selectedPath;
                else if (tag == "target")
                    vm.NewJobTarget = selectedPath;
                else if (tag == "crypto")
                    vm.CryptoSoftPath = selectedPath;
            }
        }
    }
}