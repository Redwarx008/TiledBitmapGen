using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TiledBitmapGen.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [DllImport("TildBitMapLib.dll")]
        private static extern int Add(int a, int b);

        #region property
        private string _filePath = string.Empty;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        #endregion

        #region Command
        public RelayCommand OpenFileCommand { get; }

        private async void OpenPngFile()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
    desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Png File",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[] { FilePickerFileTypes.ImagePng },
            });

            if (files != null)
            {
                FilePath = files[0].Path.AbsolutePath.ToString();
            }
        }

        public RelayCommand GenerateCommand { get; }

        int res;
        private void AddHandler()
        {
            res = Add(0, 8);
        }

        #endregion
        public MainWindowViewModel() 
        {
            OpenFileCommand = new RelayCommand(OpenPngFile);
            GenerateCommand = new RelayCommand(AddHandler);
        }
    }
}