using OpenCvSharp;

namespace FaceRecognition.Vision;

/// <summary>Algılanan bir yüzün konumu ve (model varsa) tahmin sonucu.</summary>
public sealed record AnalyzedFace(Rect Bounds, FacePrediction? Prediction);

/// <summary>Algılama ve tanıma adımlarını tek bir işlemde birleştirir.</summary>
public sealed class FaceAnalyzer
{
    private readonly FaceDetector _detector;
    private readonly FaceRecognizerService _recognizer;

    public FaceAnalyzer(FaceDetector detector, FaceRecognizerService recognizer)
    {
        _detector = detector;
        _recognizer = recognizer;
    }

    /// <summary>Karedeki tüm yüzleri bulur ve her biri için tahmin üretir.</summary>
    public IReadOnlyList<AnalyzedFace> Analyze(Mat colorImage)
    {
        var results = new List<AnalyzedFace>();
        foreach (Rect bounds in _detector.Detect(colorImage))
        {
            using Mat prepared = FacePreprocessor.Prepare(colorImage, bounds);
            results.Add(new AnalyzedFace(bounds, _recognizer.Predict(prepared)));
        }
        return results;
    }
}
