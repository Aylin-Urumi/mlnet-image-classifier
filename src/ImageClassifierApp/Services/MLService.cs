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
        // Path logic: We use the path passed from the ViewModel directly.
        // It should point to the "Data" folder containing folders 0-9.
        var images = LoadImagesFromDirectory(trainDataPath);
        var trainData = _mlContext.Data.LoadFromEnumerable(images);

        // Standard Image Classification Pipeline
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(_mlContext.Transforms.LoadImages("ImageObject", imageFolder: null, inputColumnName: "ImagePath"))
            .Append(_mlContext.Transforms.ResizeImages("ImageResized", imageWidth: 224, imageHeight: 224, inputColumnName: "ImageObject"))
            .Append(_mlContext.Transforms.ExtractPixels("Features", "ImageResized"))
            .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy("Label", "Features"))
            // We tell it to map the key back to the original folder name (0-9)
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

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

        // 1. Get all subdirectories (0, 1, 2... 9)
        var directories = Directory.GetDirectories(folder);

        foreach (var dir in directories)
        {
            string label = Path.GetFileName(dir); // Gets the folder name as the label
            
            // 2. Get only valid image files in this specific folder
            var files = Directory.GetFiles(dir)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                .Where(f => !Path.GetFileName(f).StartsWith(".")); // EXPLICITLY ignore hidden Mac files

            foreach (var file in files)
            {
                images.Add(new ImageData
                {
                    ImagePath = file,
                    Label = label
                });
            }
        }

        if (images.Count == 0)
            throw new Exception($"No valid images found in {folder}. Ensure subfolders 0-9 contain images.");

        Console.WriteLine($"Successfully loaded {images.Count} images for training.");
        return images;
    }
}