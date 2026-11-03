using System.Drawing.Drawing2D;
using FaceRecognition.Vision;
using OpenCvSharp;

namespace FaceRecognition;

/// <summary>
/// Kareyi Bitmap'e çevirip yüz dikdörtgenlerini ve etiketlerini çizer.
/// Yazı GDI+ ile çizilir çünkü Cv2.PutText Türkçe karakterleri desteklemez.
/// </summary>
public static class FrameRenderer
{
    private static readonly System.Drawing.Color KnownColor = System.Drawing.Color.FromArgb(34, 197, 94);
    private static readonly System.Drawing.Color UnknownColor = System.Drawing.Color.FromArgb(245, 158, 11);
    private static readonly System.Drawing.Color DetectedColor = System.Drawing.Color.FromArgb(56, 189, 248);

    public static Bitmap Render(Mat frame, IReadOnlyList<AnalyzedFace> faces)
    {
        Bitmap bitmap = ToBitmap(frame);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Yazı boyutu görüntü boyutuna göre ölçeklenir; yüksek çözünürlükte de okunur kalır.
        float fontSize = Math.Max(14f, bitmap.Width / 40f);
        float penWidth = Math.Max(2f, bitmap.Width / 300f);
        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);

        foreach (AnalyzedFace face in faces)
        {
            DrawFace(graphics, face, font, penWidth);
        }

        return bitmap;
    }

    /// <summary>
    /// Mat'i BMP olarak belleğe yazıp Bitmap'e çevirir. OpenCvSharp4.Extensions paketi kullanılmaz;
    /// o paketin getirdiği .NET 10 System.Drawing.Common, .NET 9 WinForms ile açılışta çakışıyor.
    /// </summary>
    private static Bitmap ToBitmap(Mat frame)
    {
        Cv2.ImEncode(".bmp", frame, out byte[] bytes);
        using var stream = new MemoryStream(bytes);
        using var decoded = new Bitmap(stream);

        // Yeni Bitmap, akıştan bağımsız ve üzerine çizilebilir bir kopyadır.
        return new Bitmap(decoded);
    }

    private static void DrawFace(Graphics graphics, AnalyzedFace face, Font font, float penWidth)
    {
        System.Drawing.Color color = face.Prediction switch
        {
            null => DetectedColor,
            { IsKnown: true } => KnownColor,
            _ => UnknownColor,
        };

        var bounds = new Rectangle(face.Bounds.X, face.Bounds.Y, face.Bounds.Width, face.Bounds.Height);
        using var pen = new Pen(color, penWidth);
        graphics.DrawRectangle(pen, bounds);

        string caption = FormatCaption(face.Prediction);
        SizeF size = graphics.MeasureString(caption, font);
        float captionY = Math.Max(0, bounds.Y - size.Height);

        using var background = new SolidBrush(color);
        graphics.FillRectangle(background, bounds.X, captionY, size.Width, size.Height);
        graphics.DrawString(caption, font, Brushes.Black, bounds.X, captionY);
    }

    private static string FormatCaption(FacePrediction? prediction)
    {
        if (prediction is null)
        {
            return "Yüz";
        }

        return prediction.IsKnown
            ? $"{prediction.Label} %{prediction.ConfidencePercent:0}"
            : prediction.Label;
    }
}
