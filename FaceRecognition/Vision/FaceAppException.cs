namespace FaceRecognition.Vision;

/// <summary>Kullanıcıya gösterilebilecek anlaşılır mesaj taşıyan uygulama hatası.</summary>
public sealed class FaceAppException(string message, Exception? innerException = null)
    : Exception(message, innerException);
