using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using ImageClassifierApp.Models;

namespace ImageClassifierApp.Services;

public class MLService
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;

    public bool IsTrained => _model != null;

    public MLService()
    {
        _mlContext = new MLContext(seed: 1);
    }

    public void TrainModel(string trainDataPath)
    {
        // 1. Force the path to look at the 'Data' folder (Capital D)
        // We combine the project path with the actual folder name in your sidebar
        string correctedPath = Path.Combine(Path.GetDirectoryName(trainDataPath)!, "Data");

        var images = LoadImagesFromDirectory(correctedPath);
        var trainData = _mlContext.Data.LoadFromEnumerable(images);

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            // Use null here so it uses the absolute paths we found
            .Append(_mlContext.Transforms.LoadImages("ImageObject", imageFolder: null, inputColumnName: "ImagePath"))
            .Append(_mlContext.Transforms.ResizeImages("ImageResized", imageWidth: 224, imageHeight: 224, inputColumnName: "ImageObject"))
            .Append(_mlContext.Transforms.ExtractPixels("Features", "ImageResized"))
            .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _model = pipeline.Fit(trainData);
    }
    public ImagePrediction Predict(string imagePath)
    {
        if (_model == null) throw new Exception("Model not trained!");

        var engine = _mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(_model);
        return engine.Predict(new ImageData { ImagePath = imagePath });
    }

    private IEnumerable<ImageData> LoadImagesFromDirectory(string folder)
    {
        var images = new List<ImageData>();
        var extensions = new[] { ".jpg", ".jpeg", ".png" };

        // Get all files, then filter out anything that isn't a standard image
        var allFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);

        foreach (var file in allFiles)
        {
            var ext = Path.GetExtension(file).ToLower();
            if (extensions.Contains(ext))
            {
                images.Add(new ImageData
                {
                    ImagePath = file,
                    Label = Directory.GetParent(file)!.Name
                });
            }
        }

        if (images.Count == 0)
            throw new Exception($"No valid images found in {folder}. Check your subfolders!");

        Console.WriteLine($"Loaded {images.Count} images for training.");
        return images;
    }
}