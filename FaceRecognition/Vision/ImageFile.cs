using OpenCvSharp;

namespace FaceRecognition.Vision;

/// <summary>
/// Görüntü dosyası okuma/yazma. Cv2.ImRead Türkçe karakterli yollarda başarısız olabildiği için
/// dosya .NET ile bayt olarak okunur ve OpenCV'ye bellekten verilir.
/// </summary>
public static class ImageFile
{
    public static Mat Read(string path)
    {
        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new FaceAppException($"Görüntü dosyası okunamadı:\n{path}", ex);
        }

        Mat image = Cv2.ImDecode(bytes, ImreadModes.Color);
        if (image.Empty())
        {
            image.Dispose();
            throw new FaceAppException($"Dosya geçerli bir görüntü değil:\n{path}");
        }

        return image;
    }

    public static void WritePng(string path, Mat image)
    {
        Cv2.ImEncode(".png", image, out byte[] bytes);
        File.WriteAllBytes(path, bytes);
    }
}
