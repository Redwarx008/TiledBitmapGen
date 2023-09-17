using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using TiledBitmapGen.Attributions;
using TiledBitmapGen.Models;
using TiledBitmapGen.Service;

namespace TiledBitmapGen.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        #region property

        [ObservableProperty]
        private string _filePath = string.Empty;


        [ObservableProperty]    
        private bool _isHeightmap = false;

        [ObservableProperty]    
        private bool _generateNormalmap = true;

        public int[] TileSizeCandidates { get; }

        private int _tileSizeSelectedIndex = 2;
        public int TileSizeSelectedIndex
        {
            get => _tileSizeSelectedIndex;
            set => SetProperty(ref _tileSizeSelectedIndex, value);  
        }

        private string _minHeight = string.Empty;
        [NumberValidation(-9999, 9999)]
        public string MinHeight
        {
            get => _minHeight;
            set
            {
                SetProperty(ref _minHeight, value);
            }
        }

        private string _maxHeight = string.Empty;
        [NumberValidation(-9999, 9999)]
        public string MaxHeight
        {
            get => _maxHeight;
            set => SetProperty(ref _maxHeight, value);  
        }

        public ObservableCollection<ErrorMessage> ErrorMessages { get; set; }

        #endregion

        #region Command

        private AsyncRelayCommand? _openFileCommand;
        public IAsyncRelayCommand OpenFileCommand => _openFileCommand ??= new AsyncRelayCommand(OpenPngFile);

        private async Task OpenPngFile()
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

            if (files != null && files.Count > 0)
            {
                FilePath = files[0].Path.LocalPath;
            }

            int nChannel = 0;
            int bitDepth = 0;
            bool res = await Task.Run(() => NativeUtility.GetImgInfo(FilePath, ref nChannel, ref bitDepth));
            if (!res)
            {
                Error("Get image info failed.");
            }
            if (nChannel == 1) 
            {
                if (bitDepth == 16)
                {
                    IsHeightmap = true;
                }
                else
                {
                    Error("Heightmap must be 16bit.");
                }
            }
        }

        public void Generate(object? parameter)
        {
            Config config = new Config()
            {
                fileName = FilePath,
                isHeightmap = IsHeightmap,
                createNormalmap = GenerateNormalmap,
                tileSize = TileSizeCandidates[_tileSizeSelectedIndex],
                minHeight = float.Parse(MinHeight),
                maxHeight = float.Parse(MaxHeight),
            };
            NativeUtility.Create(config);
        }
        [DependsOn(nameof(MinHeight))]
        [DependsOn(nameof(MaxHeight))]  
        [DependsOn(nameof(IsHeightmap))]
        [DependsOn(nameof(FilePath))]
        private bool CanGenerate(object? parameter)
        {
            if (FilePath.Length == 0)
            {
                return false;
            }
            if (IsHeightmap)
            {
                if (string.IsNullOrEmpty(MinHeight) || string.IsNullOrEmpty(MaxHeight))
                    return false;
            }
            return true;   
        }
        #endregion
        public MainWindowViewModel()
        {
            ErrorMessages = new ObservableCollection<ErrorMessage>();
            TileSizeCandidates = new int[3] { 64, 128, 256 };
        }

        private void Error(string message)
        {
            ErrorMessages.Add(new ErrorMessage(ErrorMessages.Count, message));  
        }
    }
}