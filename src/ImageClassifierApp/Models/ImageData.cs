using System;
using Microsoft.ML.Data;

namespace ImageClassifierApp.Models;

public class ImageData
{
    // ML.NET will use these property names as Column Names automatically
    public string ImagePath { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class ImagePrediction
{
    // This matches the final step in our pipeline
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; } = string.Empty;

    // This contains the probability for each digit (0-9)
    public float[] Score { get; set; } = Array.Empty<float>();
}