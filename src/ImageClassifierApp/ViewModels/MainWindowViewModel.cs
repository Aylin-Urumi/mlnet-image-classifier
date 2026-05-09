using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifierApp.Services;

namespace ImageClassifierApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly MLService _mlService = new();

    [ObservableProperty] private string _predictionResult = "Initializing...";
    [ObservableProperty] private Bitmap? _selectedImage;
    [ObservableProperty] private bool _isBusy;

    public MainWindowViewModel() => Task.Run(InitializeAsync);

    private async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            string? currentDir = AppContext.BaseDirectory;
            string? projectSourceDir = null;

            while (!string.IsNullOrEmpty(currentDir))
            {
                if (Directory.GetFiles(currentDir, "*.csproj").Any())
                {
                    projectSourceDir = currentDir;
                    break;
                }
                currentDir = Path.GetDirectoryName(currentDir);
            }

            if (!string.IsNullOrEmpty(projectSourceDir))
            {
                string dataPath = Path.Combine(projectSourceDir, "Data");

                if (!Directory.Exists(dataPath))
                    throw new Exception($"Data folder missing at: {dataPath}");

                PredictionResult = "Training AI on digits... please wait.";
                await Task.Run(() => _mlService.TrainModel(dataPath));
                PredictionResult = "Training Complete! Ready to classify.";
            }
        }
        catch (Exception ex)
        {
            PredictionResult = $"Error: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SelectImage()
    {
        if (IsBusy || !_mlService.IsTrained) return;

        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a Digit Image",
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                using (var stream = await file.OpenReadAsync())
                {
                    SelectedImage = new Bitmap(stream);
                }

                PredictionResult = "Analyzing...";
                var result = await Task.Run(() => _mlService.Predict(file.Path.LocalPath));
                var confidence = result.Score.Max() * 100;
                PredictionResult = $"I think this is a: {result.PredictedLabel} ({confidence:0.#}%)";
            }
        }
    }
    [RelayCommand]
    private void Clear()
    {
        SelectedImage = null;
        PredictionResult = "Ready to classify.";
    }
}