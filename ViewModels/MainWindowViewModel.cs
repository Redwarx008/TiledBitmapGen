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
        private int _width = 0;
        private int _height = 0;    

        #region property

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GenerateCommand))]
        private string _filePath = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GenerateCommand))] 
        private bool _isHeightmap = false;

        [ObservableProperty]    
        private bool _generateNormalmap = true;

        [ObservableProperty]
        private int _tileBorderWidthSelectedIndex = 0;

        [ObservableProperty]
        private int _tileSizeSelectedIndex = 2;

        private int _leafNodeSizeSelectedIndex = 2;

        public int LeafNodeSizeSelectedIndex
        {
            get => _leafNodeSizeSelectedIndex;  
            set
            {
                if (SetProperty(ref _leafNodeSizeSelectedIndex, value))
                {
                    int leafNodeSize = LeafNodeSizeCandidates[value];
                    LodLevelCount = CalcLodLevels(leafNodeSize).ToString();  
                }
            }
        }

        private string _minHeight = string.Empty;
        [NumberValidation(-999, 999)]
        public string MinHeight
        {
            get => _minHeight;
            set
            {
                if (SetProperty(ref _minHeight, value))
                {
                    GenerateCommand.NotifyCanExecuteChanged();  
                }
            }
        }

        private string _maxHeight = string.Empty;
        [NumberValidation(-999, 999)]
        public string MaxHeight
        {
            get => _maxHeight;
            set
            {
                if (SetProperty(ref _maxHeight, value))
                {
                    GenerateCommand.NotifyCanExecuteChanged();
                }
            }
        }

        [ObservableProperty]
        private string _lodLevelCount = string.Empty;

        [ObservableProperty]
        private string _resolution = string.Empty;

        [ObservableProperty]    
        private string _channels = string.Empty;

        [ObservableProperty]
        private string _bitDepth = string.Empty;

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

            bool res = await Task.Run(() => NativeUtility.GetImgInfo(FilePath, ref _width, ref _height, ref nChannel, ref bitDepth));
            if (!res)
            {
                Error("Get image info failed.");
            }

            LodLevelCount = CalcLodLevels(LeafNodeSizeCandidates[LeafNodeSizeSelectedIndex]).ToString();
            Resolution = $"{_width}x{_height}";
            Channels = nChannel.ToString();
            BitDepth = bitDepth.ToString(); 

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

        private IRelayCommand? _generateCommand;
        public IRelayCommand GenerateCommand => _generateCommand ??= new RelayCommand(Generate, CanGenerate);

        private void Generate()
        {
            Config config = new Config()
            {
                fileName = FilePath,
                isHeightmap = IsHeightmap,
                createNormalmap = GenerateNormalmap,
                tileSize = TileSizeCandidates[TileSizeSelectedIndex],
                minHeight = float.Parse(MinHeight),
                maxHeight = float.Parse(MaxHeight),
                lodLevelCount = int.Parse(LodLevelCount),
                leafQuadTreeNodeSize = LeafNodeSizeCandidates[LeafNodeSizeSelectedIndex],
                tileBorderWidth = TileBorderWidthCandidates[TileBorderWidthSelectedIndex],
            };
            NativeUtility.Create(config);
        }
        //[DependsOn(nameof(MinHeight))]
        //[DependsOn(nameof(MaxHeight))]  
        //[DependsOn(nameof(IsHeightmap))]
        //[DependsOn(nameof(FilePath))]
        private bool CanGenerate()
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


        public int[] TileSizeCandidates { get; }

        public int[] LeafNodeSizeCandidates { get; }

        public int[] TileBorderWidthCandidates { get; }

        public MainWindowViewModel()
        {
            ErrorMessages = new ObservableCollection<ErrorMessage>();
            TileSizeCandidates = new int[4] { 64, 128, 256, 512};
            LeafNodeSizeCandidates = new int[3] { 8, 16, 32 };
            TileBorderWidthCandidates = new int[3] { 1, 2, 4 };
        }

        private void Error(string message)
        {
            ErrorMessages.Add(new ErrorMessage(ErrorMessages.Count, message));  
        }

        private int CalcLodLevels(int leafNodeSize)
        {
            int min = Math.Min(_width, _height);
            int lodLevelCount = 0;
            while (leafNodeSize < min)
            {
                leafNodeSize <<= 1;
                ++lodLevelCount;
            }
            return lodLevelCount;   
        }
    }
}