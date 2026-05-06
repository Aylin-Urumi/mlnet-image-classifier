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
        var images = LoadImagesFromDirectory(trainDataPath);
        var trainData = _mlContext.Data.LoadFromEnumerable(images);

        // Pipeline: Load -> Resize -> Map Labels -> Train -> Map Back
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(_mlContext.Transforms.LoadImages("ImageResized", imageFolder: trainDataPath, inputColumnName: "ImagePath"))
            .Append(_mlContext.Transforms.ResizeImages("ImageResized", imageWidth: 224, imageHeight: 224))
            .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy("Label", "ImageResized"))
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
        var types = new[] { "*.jpg", "*.jpeg", "*.png" };
        return types.SelectMany(ext => Directory.GetFiles(folder, ext, SearchOption.AllDirectories))
                    .Select(file => new ImageData 
                    { 
                        ImagePath = file, 
                        Label = Directory.GetParent(file)!.Name 
                    });
    }
}