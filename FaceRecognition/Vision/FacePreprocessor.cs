using OpenCvSharp;

namespace FaceRecognition.Vision;

/// <summary>
/// Eğitimde ve tahminde aynı ön işlemi uygular. İkisi farklı olursa model tutarsız sonuç verir.
/// </summary>
public static class FacePreprocessor
{
    public const int FaceSize = 200;

    /// <summary>Yüz bölgesini kırpar, griye çevirir, ışığı dengeler ve sabit boyuta getirir.</summary>
    public static Mat Prepare(Mat colorImage, Rect faceBounds)
    {
        using var face = new Mat(colorImage, faceBounds);
        using var gray = new Mat();
        Cv2.CvtColor(face, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(gray, gray);

        var resized = new Mat();
        Cv2.Resize(gray, resized, new OpenCvSharp.Size(FaceSize, FaceSize));
        return resized;
    }
}
