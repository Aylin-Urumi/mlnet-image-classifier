using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls; // Added for TopLevel
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifierApp.Services;

namespace ImageClassifierApp.ViewModels;

// MUST BE PARTIAL for the Community Toolkit to work!
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly MLService _mlService = new();

    [ObservableProperty] 
    private string _predictionResult = "Initializing...";

    [ObservableProperty] 
    private Bitmap? _selectedImage;

    [ObservableProperty] 
    private bool _isBusy;

    public MainWindowViewModel()
    {
        // Start training immediately when app opens
        Task.Run(InitializeAsync);
    }

    private async Task InitializeAsync()
    {
        IsBusy = true;
        PredictionResult = "Training AI on digits 0-9...";
        
        try 
        {
            // Path logic: Go up from bin/Debug/netX.X/osx-arm64 to the project root
            string dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
            
            await Task.Run(() => _mlService.TrainModel(dataPath));
            PredictionResult = "Training Complete! Ready to classify.";
        }
        catch (Exception ex)
        {
            PredictionResult = $"Error: {ex.Message}";
        }
        finally 
        { 
            IsBusy = false; 
        }
    }

    [RelayCommand]
    private async Task SelectImage()
    {
        if (IsBusy || !_mlService.IsTrained) return;

        // Get the main window reference for the file picker
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
                
                // Display image
                using (var stream = await file.OpenReadAsync())
                {
                    SelectedImage = new Bitmap(stream);
                }

                // Predict
                PredictionResult = "Analyzing...";
                var result = await Task.Run(() => _mlService.Predict(file.Path.LocalPath));
                PredictionResult = $"I think this is a: {result.PredictedLabel}";
            }
        }
    }
}