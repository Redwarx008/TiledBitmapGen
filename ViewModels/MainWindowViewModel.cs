using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiledBitmapGen.Models;

namespace TiledBitmapGen.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [DllImport("TildBitMapLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Create(Config config);

        [DllImport("TildBitMapLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GetImgInfo(string fileName, ref int nChannel, ref int bitDepth);
        #region property
        private string _filePath = string.Empty;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        private bool _isHeightmap = false;
        public bool IsHeightmap
        {
            get => _isHeightmap;
            set => SetProperty(ref _isHeightmap, value);
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
                FilePath = files[0].Path.LocalPath;
            }

            int nChannel = 0;
            int bitDepth = 0;
            GetImgInfo(_filePath, ref nChannel, ref bitDepth);
            if (nChannel == 1 && bitDepth == 16) 
            {
                IsHeightmap = true;
            }
        }

        public RelayCommand GenerateCommand { get; }

        private void Generate()
        {
            Config config = new Config()
            {
                FileName = _filePath,
                IsHeightmap = true,
            };
        }
        #endregion
        public MainWindowViewModel() 
        {
            OpenFileCommand = new RelayCommand(OpenPngFile);
            //GenerateCommand = new RelayCommand(AddHandler);
        }
    }
}