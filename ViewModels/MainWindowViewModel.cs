using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;
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
    public class DelegateCommand : ICommand
    {
        private readonly Action _executeAction;
        private readonly Func<bool> _canExecuteAction;

        public DelegateCommand(Action executeAction, Func<bool> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public void Execute(object parameter) => _executeAction();

        public bool CanExecute(object parameter) => _canExecuteAction?.Invoke() ?? true;

        public event EventHandler CanExecuteChanged;

        public void InvokeCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    public partial class MainWindowViewModel : ViewModelBase
    {
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
            set
            {
                SetProperty(ref _isHeightmap, value);
            }
        }

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
            bool res = await Task.Run(() => NativeUtility.GetImgInfo(_filePath, ref nChannel, ref bitDepth));
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

        private ICommand? _generateCommand;
        public ICommand GenerateCommand => _generateCommand ??= new DelegateCommand(Generate, CanGenerate);

        private void Generate()
        {
            Config config = new Config()
            {
                fileName = FilePath,
                isHeightmap = IsHeightmap,
                tileSize = TileSizeCandidates[_tileSizeSelectedIndex],
                minHeight = float.Parse(MinHeight),
                maxHeight = float.Parse(MaxHeight),
            };
        }
        [DependsOn(nameof(FilePath))]   
        private bool CanGenerate()
        {
            //if (IsHeightmap)
            //{
            //    return !string.IsNullOrEmpty(MinHeight) || !string.IsNullOrEmpty(MaxHeight);
            //}
            if (FilePath.Length > 0) 
            {
                return true;
            } 
            else
            { return false; }   
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