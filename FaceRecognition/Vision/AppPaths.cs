namespace FaceRecognition.Vision;

/// <summary>
/// Uygulamanın kullandığı klasörler. Yüz örnekleri kişisel veri olduğundan proje klasörüne değil,
/// kullanıcının yerel uygulama klasörüne yazılır; böylece yanlışlıkla GitHub'a yüklenmez.
/// </summary>
public static class AppPaths
{
    public static string DataRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YMH219-FaceRecognition");

    public static string Dataset => Path.Combine(DataRoot, "dataset");
    public static string Model => Path.Combine(DataRoot, "model");
    public static string SavedFaces => Path.Combine(DataRoot, "saved-faces");

    public static string Cascade => Path.Combine(
        AppContext.BaseDirectory,
        "Assets",
        "haarcascade_frontalface_default.xml");
}
