using Microsoft.ML.Data;
using System;

namespace ImageClassifierApp.Models;

public class ImageData
{
    [ColumnName("ImagePath")]
    public string ImagePath { get; set; } = string.Empty;

    [ColumnName("Label")]
    public string Label { get; set; } = string.Empty;
}

public class ImagePrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; } = string.Empty;

    [ColumnName("Score")]
    public float[] Score { get; set; } = Array.Empty<float>();
}