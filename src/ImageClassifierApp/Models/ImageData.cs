using System;
using Microsoft.ML.Data;

namespace ImageClassifierApp.Models;

public class ImageData
{
    [LoadColumn(0)]
    public string ImagePath { get; set; } = string.Empty;

    [LoadColumn(1)]
    public string Label { get; set; } = string.Empty;
}

public class ImagePrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; } = string.Empty;

    public float[] Score { get; set; } = Array.Empty<float>();
}