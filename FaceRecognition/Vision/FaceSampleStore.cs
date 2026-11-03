using OpenCvSharp;

namespace FaceRecognition.Vision;

/// <summary>Bir etiket (kişi) ve o etikete ait örnek sayısı.</summary>
public sealed record LabelInfo(string Name, int SampleCount)
{
    public override string ToString() => $"{Name} ({SampleCount} örnek)";
}

/// <summary>
/// Yerel örnek kümesini yönetir. Her etiket bir klasördür: dataset/&lt;etiket&gt;/*.png
/// </summary>
public sealed class FaceSampleStore
{
    public const int MaxLabelLength = 30;

    private readonly string _root;

    public FaceSampleStore(string root)
    {
        _root = root;
    }

    /// <summary>Etiket klasör adı olarak güvenli mi? Sorun varsa açıklamasını döner, yoksa null.</summary>
    public static string? ValidateLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return "Etiket boş olamaz.";
        }
        if (label.Length > MaxLabelLength)
        {
            return $"Etiket en fazla {MaxLabelLength} karakter olabilir.";
        }
        if (label.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || label.Trim('.', ' ').Length == 0)
        {
            return "Etiket dosya adında kullanılamayan karakterler içeriyor.";
        }
        return null;
    }

    public IReadOnlyList<LabelInfo> GetLabels()
    {
        if (!Directory.Exists(_root))
        {
            return Array.Empty<LabelInfo>();
        }

        return Directory.GetDirectories(_root)
            .Select(directory => new LabelInfo(
                Path.GetFileName(directory),
                Directory.GetFiles(directory, "*.png").Length))
            .Where(info => info.SampleCount > 0)
            .OrderBy(info => info.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>Ön işlenmiş yüzü verilen etiketin klasörüne kaydeder.</summary>
    public void AddSample(string label, Mat preparedFace)
    {
        string? error = ValidateLabel(label);
        if (error is not null)
        {
            throw new FaceAppException(error);
        }

        try
        {
            string directory = Path.Combine(_root, label);
            Directory.CreateDirectory(directory);

            string fileName = $"{DateTime.Now:yyyyMMdd-HHmmss-fff}.png";
            ImageFile.WritePng(Path.Combine(directory, fileName), preparedFace);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new FaceAppException($"Örnek kaydedilemedi:\n{ex.Message}", ex);
        }
    }

    public void DeleteLabel(string label)
    {
        if (ValidateLabel(label) is not null)
        {
            return;
        }

        try
        {
            string directory = Path.Combine(_root, label);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new FaceAppException($"Etiket silinemedi:\n{ex.Message}", ex);
        }
    }

    /// <summary>Eğitim için tüm örnekleri griye çevrilmiş olarak yükler. Çağıran Mat'leri serbest bırakır.</summary>
    public IReadOnlyList<(string Label, Mat Image)> LoadSamples()
    {
        var samples = new List<(string, Mat)>();
        foreach (LabelInfo info in GetLabels())
        {
            foreach (string file in Directory.GetFiles(Path.Combine(_root, info.Name), "*.png"))
            {
                using Mat color = ImageFile.Read(file);
                var gray = new Mat();
                Cv2.CvtColor(color, gray, ColorConversionCodes.BGR2GRAY);
                samples.Add((info.Name, gray));
            }
        }
        return samples;
    }
}
