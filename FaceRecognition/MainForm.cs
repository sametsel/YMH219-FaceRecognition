using FaceRecognition.Vision;
using OpenCvSharp;

namespace FaceRecognition;

public partial class MainForm : Form
{
    private readonly FaceSampleStore _sampleStore = new(AppPaths.Dataset);
    private readonly FaceRecognizerService _recognizer = new(AppPaths.Model);
    private readonly CameraService _camera = new();

    private FaceDetector? _detector;
    private FaceAnalyzer? _analyzer;

    // Son kare ve yüzleri "Örneğe Ekle" gibi işlemler için saklanır; kamera görevi de eriştiği için kilitlenir.
    private readonly object _frameSync = new();
    private Mat? _currentFrame;
    private IReadOnlyList<AnalyzedFace> _currentFaces = Array.Empty<AnalyzedFace>();

    private int _framePending; // Arayüz yetişemezse kareler biriktirilmez, atlanır.
    private bool _isBusy;
    private bool _closingAfterStop;

    public MainForm()
    {
        InitializeComponent();

        _camera.FrameCaptured += Camera_FrameCaptured;
        _camera.Failed += Camera_Failed;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        LoadDetector();
        LoadModel();
        RefreshLabels();
        UpdateUi();
    }

    private void LoadDetector()
    {
        try
        {
            _detector = new FaceDetector(AppPaths.Cascade);
            _analyzer = new FaceAnalyzer(_detector, _recognizer);
        }
        catch (FaceAppException ex)
        {
            ShowError(ex.Message);
            SetStatus("Yüz algılama modeli yüklenemedi", Color.FromArgb(185, 28, 28));
        }
    }

    private void LoadModel()
    {
        try
        {
            lblModel.Text = _recognizer.TryLoad()
                ? "Model: eğitilmiş model yüklendi"
                : "Model: henüz eğitilmedi (yalnızca algılama yapılır)";
        }
        catch (FaceAppException ex)
        {
            lblModel.Text = "Model: okunamadı, yeniden eğitin";
            ShowError(ex.Message);
        }
    }

    // ----- Kamera -----

    private void BtnCameraStart_Click(object? sender, EventArgs e)
    {
        try
        {
            _camera.Start((int)numCamera.Value);
            SetStatus("Kamera açık", Color.FromArgb(21, 128, 61));
        }
        catch (FaceAppException ex)
        {
            ShowError(ex.Message);
        }
        catch (OpenCVException ex)
        {
            ShowError($"Kamera başlatılamadı:\n{ex.Message}");
        }

        UpdateUi();
    }

    private async void BtnCameraStop_Click(object? sender, EventArgs e)
    {
        await _camera.StopAsync();
        SetStatus("Kamera kapalı", Color.FromArgb(75, 85, 99));
        UpdateUi();
    }

    private void Camera_FrameCaptured(object? sender, Mat frame)
    {
        // Bu metot kamera görevinde çalışır. Önceki kare hâlâ işleniyorsa bu kare atlanır.
        if (_analyzer is null || Interlocked.CompareExchange(ref _framePending, 1, 0) != 0)
        {
            return;
        }

        try
        {
            IReadOnlyList<AnalyzedFace> faces = _analyzer.Analyze(frame);
            Bitmap bitmap = FrameRenderer.Render(frame, faces);
            StoreCurrentFrame(frame.Clone(), faces);

            BeginInvoke(() =>
            {
                ShowFrame(bitmap, faces);
                Volatile.Write(ref _framePending, 0);
            });
        }
        catch (Exception ex) when (ex is OpenCVException or InvalidOperationException)
        {
            // Form kapanırken veya tek bir karede oluşan hata kamerayı düşürmemeli.
            Volatile.Write(ref _framePending, 0);
        }
    }

    private void Camera_Failed(object? sender, string message)
    {
        BeginInvoke(async () =>
        {
            await _camera.StopAsync();
            SetStatus("Kamera bağlantısı kesildi", Color.FromArgb(185, 28, 28));
            UpdateUi();
            ShowError(message);
        });
    }

    // ----- Görüntü dosyası -----

    private async void BtnSelectImage_Click(object? sender, EventArgs e)
    {
        if (_analyzer is null)
        {
            ShowError("Yüz algılama modeli yüklü olmadığı için görüntü işlenemiyor.");
            return;
        }

        if (openImageDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        // Görüntüyü kamera akışının ezmemesi için önce kamera durdurulur.
        if (_camera.IsRunning)
        {
            await _camera.StopAsync();
        }

        try
        {
            Mat image = ImageFile.Read(openImageDialog.FileName);
            ProcessStillImage(image);
            SetStatus($"Görüntü: {Path.GetFileName(openImageDialog.FileName)}", Color.FromArgb(29, 78, 216));
        }
        catch (FaceAppException ex)
        {
            ShowError(ex.Message);
        }
        catch (OpenCVException ex)
        {
            ShowError($"Görüntü işlenemedi:\n{ex.Message}");
        }

        UpdateUi();
    }

    /// <summary>Sabit görüntüyü analiz eder ve gösterir; Mat'in sahipliğini alır.</summary>
    private void ProcessStillImage(Mat image)
    {
        IReadOnlyList<AnalyzedFace> faces = _analyzer!.Analyze(image);
        Bitmap bitmap = FrameRenderer.Render(image, faces);
        StoreCurrentFrame(image, faces);
        ShowFrame(bitmap, faces);
    }

    /// <summary>Model değişince ekrandaki sabit görüntünün sonucunu tazeler.</summary>
    private void ReanalyzeCurrentFrame()
    {
        if (_camera.IsRunning || _analyzer is null)
        {
            return;
        }

        Mat? copy;
        lock (_frameSync)
        {
            copy = _currentFrame?.Clone();
        }

        if (copy is not null)
        {
            ProcessStillImage(copy);
        }
    }

    private void StoreCurrentFrame(Mat frame, IReadOnlyList<AnalyzedFace> faces)
    {
        lock (_frameSync)
        {
            _currentFrame?.Dispose();
            _currentFrame = frame;
            _currentFaces = faces;
        }
    }

    private void ShowFrame(Bitmap bitmap, IReadOnlyList<AnalyzedFace> faces)
    {
        Image? previous = picPreview.Image;
        picPreview.Image = bitmap;
        previous?.Dispose();

        lblFaceCount.Text = $"Algılanan yüz: {faces.Count}";

        // Birden fazla yüz varsa özet olarak en büyük yüzün sonucu gösterilir.
        AnalyzedFace? main = faces.MaxBy(face => face.Bounds.Width * face.Bounds.Height);
        if (main is null)
        {
            lblResult.Text = "Etiket: -";
            lblConfidence.Text = "Güven: -";
        }
        else if (main.Prediction is null)
        {
            lblResult.Text = "Etiket: model eğitilmedi";
            lblConfidence.Text = "Güven: -";
        }
        else
        {
            lblResult.Text = $"Etiket: {main.Prediction.Label}";
            lblConfidence.Text = $"Güven: %{main.Prediction.ConfidencePercent:0} (uzaklık {main.Prediction.Distance:0.0})";
        }
    }

    // ----- Örnek ekleme ve eğitim -----

    private void BtnAddSample_Click(object? sender, EventArgs e)
    {
        string label = txtLabel.Text.Trim();
        string? labelError = FaceSampleStore.ValidateLabel(label);
        if (labelError is not null)
        {
            ShowError(labelError);
            txtLabel.Focus();
            return;
        }

        try
        {
            lock (_frameSync)
            {
                AnalyzedFace? face = _currentFaces.MaxBy(item => item.Bounds.Width * item.Bounds.Height);
                if (_currentFrame is null || face is null)
                {
                    ShowError("Görüntüde yüz algılanmadı. Önce yüzün göründüğü bir kare veya resim seçin.");
                    return;
                }

                using Mat prepared = FacePreprocessor.Prepare(_currentFrame, face.Bounds);
                _sampleStore.AddSample(label, prepared);
            }

            RefreshLabels();
            lblModel.Text = "Model: yeni örnek eklendi, yeniden eğitin";
        }
        catch (FaceAppException ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void BtnTrain_Click(object? sender, EventArgs e)
    {
        _isBusy = true;
        UpdateUi();
        lblModel.Text = "Model: eğitiliyor...";

        IReadOnlyList<(string Label, Mat Image)>? samples = null;
        try
        {
            samples = _sampleStore.LoadSamples();
            IReadOnlyList<(string Label, Mat Image)> trainingSamples = samples;

            // Eğitim uzun sürebilir; arayüz donmasın diye arka planda yapılır.
            await Task.Run(() => _recognizer.Train(trainingSamples));

            lblModel.Text = $"Model: {samples.Count} örnekle eğitildi";
            ReanalyzeCurrentFrame();
        }
        catch (FaceAppException ex)
        {
            lblModel.Text = _recognizer.IsTrained ? "Model: önceki model kullanılıyor" : "Model: eğitilmedi";
            ShowError(ex.Message);
        }
        finally
        {
            if (samples is not null)
            {
                foreach ((_, Mat image) in samples)
                {
                    image.Dispose();
                }
            }

            _isBusy = false;
            UpdateUi();
        }
    }

    private void BtnDeleteLabel_Click(object? sender, EventArgs e)
    {
        if (lstLabels.SelectedItem is not LabelInfo info)
        {
            ShowError("Silmek için listeden bir etiket seçin.");
            return;
        }

        DialogResult answer = MessageBox.Show(this,
            $"'{info.Name}' etiketinin {info.SampleCount} örneği silinecek. Eğitilmiş model de silinir. Onaylıyor musunuz?",
            "Etiketi Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _sampleStore.DeleteLabel(info.Name);

            // Model silinen kişinin verisini içerdiği için o da silinir.
            _recognizer.DeleteModel();
            lblModel.Text = "Model: silindi, yeniden eğitin";
            RefreshLabels();
            ReanalyzeCurrentFrame();
        }
        catch (FaceAppException ex)
        {
            ShowError(ex.Message);
        }

        UpdateUi();
    }

    private void BtnSaveFaces_Click(object? sender, EventArgs e)
    {
        try
        {
            int count = 0;
            lock (_frameSync)
            {
                if (_currentFrame is null || _currentFaces.Count == 0)
                {
                    ShowError("Kaydedilecek algılanmış yüz yok.");
                    return;
                }

                Directory.CreateDirectory(AppPaths.SavedFaces);
                string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");
                foreach (AnalyzedFace face in _currentFaces)
                {
                    using var crop = new Mat(_currentFrame, face.Bounds);
                    ImageFile.WritePng(Path.Combine(AppPaths.SavedFaces, $"yuz-{stamp}-{++count}.png"), crop);
                }
            }

            MessageBox.Show(this, $"{count} yüz kaydedildi:\n{AppPaths.SavedFaces}", "Yüzleri Kaydet",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OpenCVException)
        {
            ShowError($"Yüzler kaydedilemedi:\n{ex.Message}");
        }
    }

    private void NumThreshold_ValueChanged(object? sender, EventArgs e)
    {
        _recognizer.DistanceThreshold = (double)numThreshold.Value;
        ReanalyzeCurrentFrame();
    }

    private void RefreshLabels()
    {
        try
        {
            lstLabels.Items.Clear();
            lstLabels.Items.AddRange(_sampleStore.GetLabels().Cast<object>().ToArray());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError($"Örnek klasörü okunamadı:\n{ex.Message}");
        }
    }

    // ----- Ortak -----

    private void UpdateUi()
    {
        bool running = _camera.IsRunning;
        bool ready = _analyzer is not null;

        btnCameraStart.Enabled = ready && !running;
        btnCameraStop.Enabled = running;
        numCamera.Enabled = !running;
        btnSelectImage.Enabled = ready;
        btnAddSample.Enabled = ready;
        btnSaveFaces.Enabled = ready;
        btnTrain.Enabled = !_isBusy;
        btnDeleteLabel.Enabled = !_isBusy;
    }

    private void SetStatus(string text, Color color)
    {
        lblStatus.Text = text;
        lblStatus.BackColor = color;
    }

    private void ShowError(string message) =>
        MessageBox.Show(this, message, "Yüz Tanıma", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_closingAfterStop || !_camera.IsRunning)
        {
            return;
        }

        // Kamera görevi bitmeden form yok edilirse BeginInvoke hata verir; önce kamera durdurulur.
        e.Cancel = true;
        _closingAfterStop = true;
        await _camera.StopAsync();
        Close();
    }

    private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        lock (_frameSync)
        {
            _currentFrame?.Dispose();
            _currentFrame = null;
        }

        _detector?.Dispose();
        _recognizer.Dispose();
    }
}
