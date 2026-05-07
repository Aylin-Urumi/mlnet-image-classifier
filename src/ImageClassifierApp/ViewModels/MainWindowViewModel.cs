using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
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
        PredictionResult = "Training AI on digits 0-9... locating data folder.";
    
        try 
        {
            // === Improved, robust path logic for macOS ===
            string? currentDir = AppContext.BaseDirectory;
            string? projectSourceDir = null;

            // Loop: Navigate up the directory tree to find a directory containing a C# project file (*.csproj)
            while (!string.IsNullOrEmpty(currentDir))
            {
                if (Directory.GetFiles(currentDir, "*.csproj").Any())
                {
                    // This is the source folder where your code lives! 'data' should be here.
                    projectSourceDir = currentDir;
                    break;
                }
                currentDir = Path.GetDirectoryName(currentDir);
            }

            // If we found the source directory, construct the final path
            if (!string.IsNullOrEmpty(projectSourceDir))
            {
                // Point directly to the 'data' folder relative to the source project
                string dataPath = Path.Combine(projectSourceDir, "data");
            
                // This line tells you exactly where the AI is looking in real-time
                PredictionResult = $"Found data at: {dataPath}. Starting training...";
            
                await Task.Run(() => _mlService.TrainModel(dataPath));
                PredictionResult = "Training Complete! Ready to classify.";
            }
            else
            {
                throw new Exception("Error: Could not find the project folder. Ensure your data structure is correct.");
            }
        }
        catch (Exception ex)
        {
            // This is why the error shows up in the UI
            PredictionResult = $"Error: {ex.Message}";
        }
        finally 
        { 
            IsBusy = false; // Training either succeeded or failed, so we are not 'busy' anymore
        }
    }

    [RelayCommand]
    private async Task SelectImage()
    {
        // 1. Guard Clause: If the AI is still training or the app is busy, do nothing.
        // If your app shows the "Error: Directory not found" from your screenshot,
        // _mlService.IsTrained will be FALSE, so this button will stay "locked."
        if (IsBusy || !_mlService.IsTrained) return;

        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel == null) return;

            // 2. Open the Mac File Picker
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a Digit Image",
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files.Count > 0)
            {
                var file = files[0];

                // 3. Display the image in the UI
                // This works because we have [ObservableProperty] on _selectedImage
                using (var stream = await file.OpenReadAsync())
                {
                    SelectedImage = new Bitmap(stream);
                }

                // 4. Run the Prediction
                PredictionResult = "Analyzing...";

                // We use Task.Run so the UI doesn't freeze while the AI "thinks"
                var result = await Task.Run(() => _mlService.Predict(file.Path.LocalPath));

                // 5. Show the result
                PredictionResult = $"I think this is a: {result.PredictedLabel}";
            }
        }
    }
}