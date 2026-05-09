using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using ImageClassifierApp.Models;

namespace ImageClassifierApp.Services;

public class MLService
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;

    public bool IsTrained => _model != null;

    public MLService() => _mlContext = new MLContext(seed: 1);

    public void TrainModel(string trainDataPath)
    {
        try
        {
            var images = LoadImagesFromDirectory(trainDataPath).ToList();

            // Print the first path to the terminal so we can see it
            if (images.Any())
                Console.WriteLine($"DEBUG: Attempting to load first image at: {images[0].ImagePath}");

            var trainData = _mlContext.Data.LoadFromEnumerable(images);

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(_mlContext.Transforms.LoadImages("ImageObject", imageFolder: null, inputColumnName: "ImagePath"))
                .Append(_mlContext.Transforms.ResizeImages("ImageResized", 28, 28, "ImageObject"))
                .Append(_mlContext.Transforms.ExtractPixels("Features", "ImageResized"))
                .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy("Label", "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            _model = pipeline.Fit(trainData);
        }
        catch (Exception ex)
        {
            Console.WriteLine("--- ML.NET ENGINE ERROR ---");
            Console.WriteLine(ex.ToString()); // This will show the FULL technical error in the terminal
            throw;
        }
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

        foreach (var directory in Directory.GetDirectories(folder))
        {
            string label = Path.GetFileName(directory);
            var files = Directory.GetFiles(directory);

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                // MAC FIX: Ignore hidden files starting with "." (like .DS_Store or ._image)
                if (!fileName.StartsWith(".") && extensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    images.Add(new ImageData { ImagePath = file, Label = label });
                }
            }
        }

        if (images.Count == 0) throw new Exception($"No images found in {folder}. Check folder naming!");
        return images;
    }
}