using OpenCvSharp;

namespace FaceRecognition.Vision;

/// <summary>Haar Cascade modeliyle görüntüdeki yüz bölgelerini bulur.</summary>
public sealed class FaceDetector : IDisposable
{
    private readonly CascadeClassifier _classifier;

    // CascadeClassifier aynı anda iki iş parçacığından çağrılmamalıdır.
    private readonly object _sync = new();

    /// <exception cref="FaceAppException">Model dosyası yoksa veya bozuksa.</exception>
    public FaceDetector(string cascadePath)
    {
        if (!File.Exists(cascadePath))
        {
            throw new FaceAppException($"Yüz algılama modeli bulunamadı:\n{cascadePath}");
        }

        _classifier = new CascadeClassifier();
        try
        {
            string xml = File.ReadAllText(cascadePath);
            using var storage = new FileStorage(xml, FileStorage.Modes.Read | FileStorage.Modes.Memory);
            using FileNode? node = storage.GetFirstTopLevelNode();

            if (node is null || !_classifier.Read(node) || _classifier.Empty())
            {
                throw new FaceAppException($"Yüz algılama modeli okunamadı:\n{cascadePath}");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OpenCVException)
        {
            _classifier.Dispose();
            throw new FaceAppException($"Yüz algılama modeli yüklenemedi:\n{cascadePath}", ex);
        }
    }

    /// <summary>Renkli (BGR) görüntüdeki yüzleri döndürür. Yüz yoksa boş dizi döner.</summary>
    public Rect[] Detect(Mat colorImage)
    {
        using var gray = new Mat();
        Cv2.CvtColor(colorImage, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(gray, gray);

        lock (_sync)
        {
            return _classifier.DetectMultiScale(
                gray,
                scaleFactor: 1.1,
                minNeighbors: 5,
                minSize: new OpenCvSharp.Size(40, 40));
        }
    }

    public void Dispose() => _classifier.Dispose();
}
