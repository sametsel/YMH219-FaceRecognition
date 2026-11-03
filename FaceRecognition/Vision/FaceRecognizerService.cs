using OpenCvSharp;
using OpenCvSharp.Face;

namespace FaceRecognition.Vision;

/// <summary>Bir yüz için tahmin sonucu.</summary>
/// <param name="Label">Tahmin edilen etiket; eşik aşıldıysa bilinmeyen sayılır.</param>
/// <param name="Distance">LBPH uzaklığı: küçük olması daha çok benzerlik demektir.</param>
/// <param name="ConfidencePercent">Eşiğe göre hesaplanan 0-100 arası yaklaşık güven skoru.</param>
public sealed record FacePrediction(string Label, bool IsKnown, double Distance, double ConfidencePercent);

/// <summary>LBPH yöntemiyle yüz tanıma modelini eğitir, saklar ve tahmin yapar.</summary>
public sealed class FaceRecognizerService : IDisposable
{
    public const string UnknownLabel = "Bilinmiyor";
    public const int MinimumLabelCount = 2;

    private const string ModelFileName = "lbph-model.yml";
    private const string LabelsFileName = "labels.txt";

    private readonly string _modelDirectory;
    private readonly object _sync = new(); // Eğitim ve kamera tahmini farklı iş parçacıklarındadır.

    private LBPHFaceRecognizer? _recognizer;
    private string[] _labels = Array.Empty<string>();

    /// <summary>Bu uzaklığın üstündeki tahminler "Bilinmiyor" sayılır.</summary>
    public double DistanceThreshold { get; set; } = 70;

    public bool IsTrained
    {
        get
        {
            lock (_sync)
            {
                return _recognizer is not null;
            }
        }
    }

    public FaceRecognizerService(string modelDirectory)
    {
        _modelDirectory = modelDirectory;
    }

    /// <summary>Örneklerden modeli eğitir ve diske yazar. Örnekleri serbest bırakmak çağıranın işidir.</summary>
    public void Train(IReadOnlyList<(string Label, Mat Image)> samples)
    {
        string[] labels = samples
            .Select(sample => sample.Label)
            .Distinct()
            .OrderBy(label => label, StringComparer.Ordinal)
            .ToArray();

        if (labels.Length < MinimumLabelCount)
        {
            throw new FaceAppException(
                $"Eğitim için en az {MinimumLabelCount} farklı etiket gerekir. Şu an: {labels.Length}.");
        }

        LBPHFaceRecognizer recognizer = LBPHFaceRecognizer.Create();
        try
        {
            // OpenCV etiketleri sayı olarak ister; etiket adının dizideki sırası kullanılır.
            int[] labelIds = samples.Select(sample => Array.IndexOf(labels, sample.Label)).ToArray();
            recognizer.Train(samples.Select(sample => sample.Image), labelIds);
            SaveModel(recognizer, labels);
        }
        catch (Exception ex) when (ex is OpenCVException or IOException or UnauthorizedAccessException)
        {
            recognizer.Dispose();
            throw new FaceAppException($"Model eğitilemedi:\n{ex.Message}", ex);
        }

        lock (_sync)
        {
            _recognizer?.Dispose();
            _recognizer = recognizer;
            _labels = labels;
        }
    }

    /// <summary>Kayıtlı modeli yükler. Model henüz yoksa false döner; dosya bozuksa hata fırlatır.</summary>
    public bool TryLoad()
    {
        string modelPath = Path.Combine(_modelDirectory, ModelFileName);
        string labelsPath = Path.Combine(_modelDirectory, LabelsFileName);

        if (!File.Exists(modelPath) || !File.Exists(labelsPath))
        {
            return false;
        }

        LBPHFaceRecognizer recognizer = LBPHFaceRecognizer.Create();
        try
        {
            string yaml = File.ReadAllText(modelPath);
            using var storage = new FileStorage(yaml, FileStorage.Modes.Read | FileStorage.Modes.Memory);
            // Model alanları dosyanın en üst seviyesinde durduğu için kök düğümden okunur.
            using FileNode root = storage.Root()
                ?? throw new FaceAppException("Tanıma modeli dosyası boş.");
            recognizer.Read(root);

            string[] labels = File.ReadAllLines(labelsPath);

            lock (_sync)
            {
                _recognizer?.Dispose();
                _recognizer = recognizer;
                _labels = labels;
            }
            return true;
        }
        catch (Exception ex) when (ex is OpenCVException or IOException or UnauthorizedAccessException or FaceAppException)
        {
            recognizer.Dispose();
            throw new FaceAppException(
                "Kayıtlı tanıma modeli okunamadı. Modeli yeniden eğitmeniz gerekiyor.", ex);
        }
    }

    /// <summary>Eğitilmiş modeli bellekten ve diskten siler (ör. bir etiket silindiğinde).</summary>
    public void DeleteModel()
    {
        lock (_sync)
        {
            _recognizer?.Dispose();
            _recognizer = null;
            _labels = Array.Empty<string>();
        }

        try
        {
            File.Delete(Path.Combine(_modelDirectory, ModelFileName));
            File.Delete(Path.Combine(_modelDirectory, LabelsFileName));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new FaceAppException($"Model dosyası silinemedi:\n{ex.Message}", ex);
        }
    }

    /// <summary>Ön işlenmiş yüz için tahmin yapar. Model eğitilmemişse null döner.</summary>
    public FacePrediction? Predict(Mat preparedFace)
    {
        lock (_sync)
        {
            if (_recognizer is null)
            {
                return null;
            }

            _recognizer.Predict(preparedFace, out int labelId, out double distance);

            bool isKnown = labelId >= 0 && labelId < _labels.Length && distance <= DistanceThreshold;
            double confidence = Math.Clamp((1 - distance / DistanceThreshold) * 100, 0, 100);

            return new FacePrediction(isKnown ? _labels[labelId] : UnknownLabel, isKnown, distance, confidence);
        }
    }

    private void SaveModel(LBPHFaceRecognizer recognizer, string[] labels)
    {
        Directory.CreateDirectory(_modelDirectory);

        // Türkçe karakterli yollarda sorun yaşamamak için model bellekten metne alınıp .NET ile yazılır.
        using var storage = new FileStorage(ModelFileName, FileStorage.Modes.Write | FileStorage.Modes.Memory);
        recognizer.Write(storage);
        string yaml = storage.ReleaseAndGetString();

        File.WriteAllText(Path.Combine(_modelDirectory, ModelFileName), yaml);
        File.WriteAllLines(Path.Combine(_modelDirectory, LabelsFileName), labels);
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _recognizer?.Dispose();
            _recognizer = null;
        }
    }
}
