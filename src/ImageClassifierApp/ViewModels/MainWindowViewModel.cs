using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageClassifierApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // [ObservableProperty] automatically creates 'PredictionResult' with notification logic
    [ObservableProperty]
    private string _predictionResult = "Select an image to begin";

    [ObservableProperty]
    private Bitmap? _selectedImage;

    // [RelayCommand] automatically creates 'SelectImageCommand' for the button
    [RelayCommand]
    private async Task SelectImage()
    {
        PredictionResult = "Opening file picker...";
        
        // Simulating work for now
        await Task.Delay(500);
        
        PredictionResult = "Waiting for file selection...";
    }
}