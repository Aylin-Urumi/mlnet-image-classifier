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

    public MLService()
    {
        // MLContext is the starting point for all ML.NET operations (like a DB connection)
        _mlContext = new MLContext(seed: 1);
    }

    // This method will look at your 'data' folder and learn
    public void TrainModel(string trainDataPath)
    {
        // 1. Load data from folders (Cactus, Fern, etc.)
        var images = LoadImagesFromDirectory(trainDataPath);
        var trainData = _mlContext.Data.LoadFromEnumerable(images);

        // 2. Define the pipeline (Image Loading -> Resizing -> Training)
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(_mlContext.Transforms.LoadImages("ImageResized", imageFolder: trainDataPath, inputColumnName: "ImagePath"))
            .Append(_mlContext.Transforms.ResizeImages("ImageResized", imageWidth: 224, imageHeight: 224))
            .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy("Label", "ImageResized")) // Simple trainer
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        // 3. Train the model
        _model = pipeline.Fit(trainData);
    }

    private IEnumerable<ImageData> LoadImagesFromDirectory(string folder)
    {
        // Logic to scan folders and label them based on folder name
        var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png"));

        foreach (var file in files)
        {
            yield return new ImageData 
            { 
                ImagePath = file, 
                Label = Directory.GetParent(file)!.Name 
            };
        }
    }
}