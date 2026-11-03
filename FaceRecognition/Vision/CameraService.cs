using OpenCvSharp;

namespace FaceRecognition.Vision;

/// <summary>
/// Kameradan arka plan görevinde kare okur. Arayüz iş parçacığı bloke olmaz;
/// kareler olayla bildirilir ve yalnızca olay süresince geçerlidir.
/// </summary>
public sealed class CameraService
{
    private const int MaxReadFailures = 30;

    private CancellationTokenSource? _stopSource;
    private Task? _loop;

    public bool IsRunning => _loop is not null;

    public event EventHandler<Mat>? FrameCaptured;

    /// <summary>Kamera beklenmedik biçimde durduğunda tetiklenir.</summary>
    public event EventHandler<string>? Failed;

    /// <exception cref="FaceAppException">Kamera bulunamaz veya açılamazsa.</exception>
    public void Start(int cameraIndex)
    {
        if (IsRunning)
        {
            return;
        }

        var capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
        if (!capture.IsOpened())
        {
            capture.Dispose();
            throw new FaceAppException(
                $"{cameraIndex} numaralı kamera açılamadı. Kameranın bağlı olduğunu, başka bir uygulamanın " +
                "kullanmadığını ve Windows gizlilik ayarlarında izin verildiğini kontrol edin.");
        }

        _stopSource = new CancellationTokenSource();
        CancellationToken token = _stopSource.Token;
        _loop = Task.Run(() => ReadLoop(capture, token));
    }

    public async Task StopAsync()
    {
        if (_loop is null || _stopSource is null)
        {
            return;
        }

        _stopSource.Cancel();
        await _loop.ConfigureAwait(false);

        _stopSource.Dispose();
        _stopSource = null;
        _loop = null;
    }

    private void ReadLoop(VideoCapture capture, CancellationToken cancellationToken)
    {
        using (capture)
        using (var frame = new Mat())
        {
            int failures = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (capture.Read(frame) && !frame.Empty())
                {
                    failures = 0;
                    FrameCaptured?.Invoke(this, frame);
                    continue;
                }

                if (++failures >= MaxReadFailures)
                {
                    Failed?.Invoke(this, "Kameradan görüntü alınamıyor. Bağlantısı kesilmiş olabilir.");
                    return;
                }

                Thread.Sleep(30);
            }
        }
    }
}
