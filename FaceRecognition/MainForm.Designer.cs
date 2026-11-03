namespace FaceRecognition;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblStatus = new Label();
        picPreview = new PictureBox();
        lblCamera = new Label();
        numCamera = new NumericUpDown();
        btnCameraStart = new Button();
        btnCameraStop = new Button();
        btnSelectImage = new Button();
        grpResult = new GroupBox();
        lblFaceCount = new Label();
        lblResult = new Label();
        lblConfidence = new Label();
        lblThreshold = new Label();
        numThreshold = new NumericUpDown();
        grpTraining = new GroupBox();
        lblLabel = new Label();
        txtLabel = new TextBox();
        btnAddSample = new Button();
        lstLabels = new ListBox();
        btnTrain = new Button();
        btnDeleteLabel = new Button();
        lblModel = new Label();
        btnSaveFaces = new Button();
        lblScope = new Label();
        openImageDialog = new OpenFileDialog();
        ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numCamera).BeginInit();
        grpResult.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numThreshold).BeginInit();
        grpTraining.SuspendLayout();
        SuspendLayout();
        //
        // lblStatus
        //
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.BackColor = Color.FromArgb(75, 85, 99);
        lblStatus.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblStatus.ForeColor = Color.White;
        lblStatus.Location = new Point(12, 12);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(1000, 34);
        lblStatus.Text = "Kamera kapalı";
        lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        //
        // picPreview
        //
        picPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        picPreview.BackColor = Color.FromArgb(31, 41, 55);
        picPreview.BorderStyle = BorderStyle.FixedSingle;
        picPreview.Location = new Point(12, 56);
        picPreview.Name = "picPreview";
        picPreview.Size = new Size(680, 510);
        picPreview.SizeMode = PictureBoxSizeMode.Zoom;
        picPreview.TabStop = false;
        //
        // lblCamera
        //
        lblCamera.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblCamera.AutoSize = true;
        lblCamera.Location = new Point(12, 586);
        lblCamera.Name = "lblCamera";
        lblCamera.Text = "Kamera no:";
        //
        // numCamera
        //
        numCamera.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        numCamera.Location = new Point(84, 582);
        numCamera.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
        numCamera.Name = "numCamera";
        numCamera.Size = new Size(50, 23);
        numCamera.TabIndex = 0;
        //
        // btnCameraStart
        //
        btnCameraStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnCameraStart.Location = new Point(146, 576);
        btnCameraStart.Name = "btnCameraStart";
        btnCameraStart.Size = new Size(120, 36);
        btnCameraStart.TabIndex = 1;
        btnCameraStart.Text = "Kamera Başlat";
        btnCameraStart.UseVisualStyleBackColor = true;
        btnCameraStart.Click += BtnCameraStart_Click;
        //
        // btnCameraStop
        //
        btnCameraStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnCameraStop.Location = new Point(272, 576);
        btnCameraStop.Name = "btnCameraStop";
        btnCameraStop.Size = new Size(120, 36);
        btnCameraStop.TabIndex = 2;
        btnCameraStop.Text = "Kamera Durdur";
        btnCameraStop.UseVisualStyleBackColor = true;
        btnCameraStop.Click += BtnCameraStop_Click;
        //
        // btnSelectImage
        //
        btnSelectImage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnSelectImage.Location = new Point(398, 576);
        btnSelectImage.Name = "btnSelectImage";
        btnSelectImage.Size = new Size(120, 36);
        btnSelectImage.TabIndex = 3;
        btnSelectImage.Text = "Görüntü Seç";
        btnSelectImage.UseVisualStyleBackColor = true;
        btnSelectImage.Click += BtnSelectImage_Click;
        //
        // grpResult
        //
        grpResult.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        grpResult.Controls.Add(lblFaceCount);
        grpResult.Controls.Add(lblResult);
        grpResult.Controls.Add(lblConfidence);
        grpResult.Controls.Add(lblThreshold);
        grpResult.Controls.Add(numThreshold);
        grpResult.Location = new Point(702, 50);
        grpResult.Name = "grpResult";
        grpResult.Size = new Size(310, 148);
        grpResult.TabIndex = 4;
        grpResult.TabStop = false;
        grpResult.Text = "Tanıma Sonucu";
        //
        // lblFaceCount
        //
        lblFaceCount.Location = new Point(12, 24);
        lblFaceCount.Name = "lblFaceCount";
        lblFaceCount.Size = new Size(286, 20);
        lblFaceCount.Text = "Algılanan yüz: 0";
        //
        // lblResult
        //
        lblResult.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblResult.Location = new Point(12, 46);
        lblResult.Name = "lblResult";
        lblResult.Size = new Size(286, 26);
        lblResult.Text = "Etiket: -";
        //
        // lblConfidence
        //
        lblConfidence.Location = new Point(12, 76);
        lblConfidence.Name = "lblConfidence";
        lblConfidence.Size = new Size(286, 20);
        lblConfidence.Text = "Güven: -";
        //
        // lblThreshold
        //
        lblThreshold.AutoSize = true;
        lblThreshold.Location = new Point(12, 110);
        lblThreshold.Name = "lblThreshold";
        lblThreshold.Text = "Uzaklık eşiği:";
        //
        // numThreshold
        //
        numThreshold.Location = new Point(100, 106);
        numThreshold.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
        numThreshold.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
        numThreshold.Name = "numThreshold";
        numThreshold.Size = new Size(70, 23);
        numThreshold.TabIndex = 0;
        numThreshold.Value = new decimal(new int[] { 70, 0, 0, 0 });
        numThreshold.ValueChanged += NumThreshold_ValueChanged;
        //
        // grpTraining
        //
        grpTraining.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        grpTraining.Controls.Add(lblLabel);
        grpTraining.Controls.Add(txtLabel);
        grpTraining.Controls.Add(btnAddSample);
        grpTraining.Controls.Add(lstLabels);
        grpTraining.Controls.Add(btnTrain);
        grpTraining.Controls.Add(btnDeleteLabel);
        grpTraining.Controls.Add(lblModel);
        grpTraining.Controls.Add(btnSaveFaces);
        grpTraining.Location = new Point(702, 204);
        grpTraining.Name = "grpTraining";
        grpTraining.Size = new Size(310, 340);
        grpTraining.TabIndex = 5;
        grpTraining.TabStop = false;
        grpTraining.Text = "Eğitim ve Veri Ekleme";
        //
        // lblLabel
        //
        lblLabel.AutoSize = true;
        lblLabel.Location = new Point(12, 28);
        lblLabel.Name = "lblLabel";
        lblLabel.Text = "Etiket:";
        //
        // txtLabel
        //
        txtLabel.Location = new Point(60, 24);
        txtLabel.MaxLength = 30;
        txtLabel.Name = "txtLabel";
        txtLabel.PlaceholderText = "Örn. Gonullu-1";
        txtLabel.Size = new Size(238, 23);
        txtLabel.TabIndex = 0;
        //
        // btnAddSample
        //
        btnAddSample.Location = new Point(12, 56);
        btnAddSample.Name = "btnAddSample";
        btnAddSample.Size = new Size(286, 34);
        btnAddSample.TabIndex = 1;
        btnAddSample.Text = "Algılanan Yüzü Örneğe Ekle";
        btnAddSample.UseVisualStyleBackColor = true;
        btnAddSample.Click += BtnAddSample_Click;
        //
        // lstLabels
        //
        lstLabels.FormattingEnabled = true;
        lstLabels.IntegralHeight = false;
        lstLabels.Location = new Point(12, 98);
        lstLabels.Name = "lstLabels";
        lstLabels.Size = new Size(286, 100);
        lstLabels.TabIndex = 2;
        //
        // btnTrain
        //
        btnTrain.Location = new Point(12, 206);
        btnTrain.Name = "btnTrain";
        btnTrain.Size = new Size(140, 34);
        btnTrain.TabIndex = 3;
        btnTrain.Text = "Modeli Eğit";
        btnTrain.UseVisualStyleBackColor = true;
        btnTrain.Click += BtnTrain_Click;
        //
        // btnDeleteLabel
        //
        btnDeleteLabel.Location = new Point(158, 206);
        btnDeleteLabel.Name = "btnDeleteLabel";
        btnDeleteLabel.Size = new Size(140, 34);
        btnDeleteLabel.TabIndex = 4;
        btnDeleteLabel.Text = "Seçili Etiketi Sil";
        btnDeleteLabel.UseVisualStyleBackColor = true;
        btnDeleteLabel.Click += BtnDeleteLabel_Click;
        //
        // lblModel
        //
        lblModel.Location = new Point(12, 248);
        lblModel.Name = "lblModel";
        lblModel.Size = new Size(286, 36);
        lblModel.Text = "Model: -";
        //
        // btnSaveFaces
        //
        btnSaveFaces.Location = new Point(12, 290);
        btnSaveFaces.Name = "btnSaveFaces";
        btnSaveFaces.Size = new Size(286, 34);
        btnSaveFaces.TabIndex = 5;
        btnSaveFaces.Text = "Algılanan Yüzleri Klasöre Kaydet";
        btnSaveFaces.UseVisualStyleBackColor = true;
        btnSaveFaces.Click += BtnSaveFaces_Click;
        //
        // lblScope
        //
        lblScope.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        lblScope.ForeColor = Color.FromArgb(75, 85, 99);
        lblScope.Location = new Point(702, 552);
        lblScope.Name = "lblScope";
        lblScope.Size = new Size(310, 60);
        lblScope.Text = "Yalnızca ders kapsamında, yerel ve gönüllü örneklerle kullanın. Örnekler bu bilgisayarda kalır; ağa gönderilmez.";
        //
        // openImageDialog
        //
        openImageDialog.Filter = "Görüntü dosyaları (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
        openImageDialog.Title = "Görüntü seçin";
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1024, 624);
        Controls.Add(lblStatus);
        Controls.Add(picPreview);
        Controls.Add(lblCamera);
        Controls.Add(numCamera);
        Controls.Add(btnCameraStart);
        Controls.Add(btnCameraStop);
        Controls.Add(btnSelectImage);
        Controls.Add(grpResult);
        Controls.Add(grpTraining);
        Controls.Add(lblScope);
        MinimumSize = new Size(1040, 663);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "OpenCV ile Yüz Algılama ve Tanıma - YMH219";
        FormClosing += MainForm_FormClosing;
        FormClosed += MainForm_FormClosed;
        ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
        ((System.ComponentModel.ISupportInitialize)numCamera).EndInit();
        grpResult.ResumeLayout(false);
        grpResult.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numThreshold).EndInit();
        grpTraining.ResumeLayout(false);
        grpTraining.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private Label lblStatus;
    private PictureBox picPreview;
    private Label lblCamera;
    private NumericUpDown numCamera;
    private Button btnCameraStart;
    private Button btnCameraStop;
    private Button btnSelectImage;
    private GroupBox grpResult;
    private Label lblFaceCount;
    private Label lblResult;
    private Label lblConfidence;
    private Label lblThreshold;
    private NumericUpDown numThreshold;
    private GroupBox grpTraining;
    private Label lblLabel;
    private TextBox txtLabel;
    private Button btnAddSample;
    private ListBox lstLabels;
    private Button btnTrain;
    private Button btnDeleteLabel;
    private Label lblModel;
    private Button btnSaveFaces;
    private Label lblScope;
    private OpenFileDialog openImageDialog;
}
