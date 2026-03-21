using M0LTE.WsjtxUdpLib.Messages;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WSPR_Sked
{
    public partial class SpectrumForm : Form
    {

            private PictureBox waterfallBox;
            private Bitmap waterfallBitmap;
            private Label freqLabel;
            private Label statusLabel;
            private Timer readTimer;
           
            private const int SAMPLE_RATE = 12000; // adjust to match your WAV file
            private const int FFT_SIZE = 16384;
            private const float FREQ_MIN = 1300f;
            private const float FREQ_MAX = 1700f;
            private float[] fftBuffer = new float[FFT_SIZE];
            private int fftPos = 0;
            private object lockObj = new object();

            public string wavPath;
            public string freq = "";

        private string lastWavPath = "";
        private long filePosition = 0;

        private TrackBar gainControl;
        private Label gainLabel;
        private float gainDb = 0f;  // gain offset in dB
        private DateTime currentFileStartTime = DateTime.MinValue;

        private bool drawTimeOnNextLine = false;
        private string pendingTimeLabel = "";

        private Label cycleTimeLabel;

        private DateTime fileCompleteTime = DateTime.MinValue;

        private bool resizing = false;


        public SpectrumForm()
        {
            this.Text = "WSPR Spectrum";
            this.Size = new Size(1200, 580);

            this.BackColor = Color.DarkSlateGray;

            // Frequency label
            freqLabel = new Label();
            freqLabel.Location = new Point(10, 10);
           freqLabel.Font = new Font(freqLabel.Font, FontStyle.Bold);
            freqLabel.ForeColor = Color.Cyan;
            freqLabel.BackColor = Color.DarkSlateGray;
            freqLabel.Width = 140;
            freqLabel.Text = "1300 - 1700 Hz  (WSPR)";
            this.Controls.Add(freqLabel);

            // Status label
            statusLabel = new Label();
            statusLabel.Location = new Point(300, 10);
            statusLabel.Font = new Font(statusLabel.Font, FontStyle.Bold);
            statusLabel.ForeColor = Color.Yellow;
            statusLabel.BackColor = Color.DarkSlateGray;
            statusLabel.Width = 150;
            statusLabel.Text = "Waiting for WAV file...";
            this.Controls.Add(statusLabel);

            // Gain label
            gainLabel = new Label();
            gainLabel.Location = new Point(580, 10);
            gainLabel.Font = new Font(gainLabel.Font, FontStyle.Bold);
            gainLabel.ForeColor = Color.White;
            gainLabel.BackColor = Color.DarkSlateGray; ;
            gainLabel.Width = 100;
            gainLabel.Text = "Gain: 0 dB";
            this.Controls.Add(gainLabel);

            // Gain trackbar
            gainControl = new TrackBar();
            gainControl.Location = new Point(680, 2);
            gainControl.Size = new Size(180, 25);
            gainControl.Minimum = -40;
            gainControl.Maximum = 40;
            gainControl.Value = 0;
            gainControl.TickFrequency = 10;
            gainControl.BackColor = Color.DarkSlateGray;
            gainControl.Scroll += (s, e) =>
            {
                gainDb = gainControl.Value;
                gainLabel.Text = $"Gain: {gainDb:+0;-0} dB";
            };
            gainControl.AutoSize = false;
            gainControl.Size = new Size(120, 25);
            this.Controls.Add(gainControl);

            cycleTimeLabel = new Label();
            cycleTimeLabel.Location = new Point(170, 10);
            cycleTimeLabel.Width = 80;
            //cycleTimeLabel.Height = 20;
            cycleTimeLabel.Font = new Font(cycleTimeLabel.Font, FontStyle.Bold);
            cycleTimeLabel.ForeColor = Color.Yellow;
            cycleTimeLabel.BackColor = Color.DarkSlateGray;
            cycleTimeLabel.AutoSize = true;
            cycleTimeLabel.Text = "00:00:00";
            this.Controls.Add(cycleTimeLabel);

            // Waterfall display
            waterfallBox = new PictureBox();
            waterfallBox.Location = new Point(10, 40);
            //waterfallBox.Size = new Size(this.ClientSize.Width - 30, 500);
            waterfallBox.Size = new Size(this.ClientSize.Width - 20, 500);
            waterfallBox.BackColor = Color.DarkSlateGray;
            waterfallBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                | AnchorStyles.Left | AnchorStyles.Right;
            waterfallBox.MouseMove += WaterfallBox_MouseMove;
            this.Controls.Add(waterfallBox);

            //waterfallBitmap = new Bitmap(this.ClientSize.Width - 30, 500);
            waterfallBitmap = new Bitmap(this.ClientSize.Width - 20, 500);
            using (var g = Graphics.FromImage(waterfallBitmap))
                g.Clear(Color.DarkSlateGray);

            this.Resize += (s, e) =>
            {
                if (this.InvokeRequired)
                    this.Invoke((Action)ResizeBitmap);
                else
                    ResizeBitmap();
            };
            this.MaximizeBox = false;

            this.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                readTimer?.Stop();
                this.Hide();
            };

            DrawFrequencyScale();

            // WAV read timer
            readTimer = new Timer();
            readTimer.Interval = 500;
            readTimer.Tick += ReadTimer_Tick;
            readTimer.Start();

        }
        private void SpectrumForm_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;
        }
       
        private void DrawFrequencyScale()
        {
            using (var g = Graphics.FromImage(waterfallBitmap))
            using (var font = new Font("Arial", 9, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Yellow))
            {
                // Background strip
                g.FillRectangle(Brushes.DarkSlateGray, 0, 0, waterfallBitmap.Width, 22);

                for (int freq = 1300; freq <= 1700; freq += 10)
                {
                    float x = (freq - FREQ_MIN) / (FREQ_MAX - FREQ_MIN) * waterfallBitmap.Width;

                    if (freq % 50 == 0)
                    {
                        // Major tick and label at every 50Hz
                        g.DrawLine(Pens.Red, x, 14, x, 22);

                        // Keep labels inside edges
                        float labelX = x - 12;
                        if (freq == 1300) labelX = 2;
                        if (freq == 1700) labelX = x - 25;

                        g.DrawString($"{freq}", font, brush, labelX, 1);

                        // Solid vertical line down through waterfall
                        for (int y = 22; y < waterfallBitmap.Height; y += 6)
                            g.DrawLine(new Pen(Color.FromArgb(120, 180, 0, 0)), x, y, x, y + 2);
                    }
                    else
                    {
                        // Minor tick at every 10Hz
                        g.DrawLine(Pens.DarkRed, x, 18, x, 22);
                        // Faint dotted vertical line
                        for (int y = 22; y < waterfallBitmap.Height; y += 10)
                            g.DrawLine(new Pen(Color.FromArgb(60, 120, 0, 0)), x, y, x, y + 2);
                    }
                }
            }
        }
        private void DrawCycleLabel(string timeStr)
        {
            lock (lockObj)
            {
                if (waterfallBitmap == null) return;
                using (var g = Graphics.FromImage(waterfallBitmap))
                using (var font = new Font("Courier New", 8, FontStyle.Bold))
                {
                    // Draw time label on the left just below the separator line
                    g.FillRectangle(Brushes.DarkSlateGray, 0, waterfallBitmap.Height - 20, 55, 14);
                    g.DrawString(timeStr, font, Brushes.Yellow, 2, waterfallBitmap.Height - 20);
                }
                waterfallBox.Image = waterfallBitmap;
            }
        }

        private void ResizeBitmap()
        {
            readTimer?.Stop();

            try
            {
                waterfallBox.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 50);
               

                int w = Math.Max(1, waterfallBox.Width);
                int h = Math.Max(1, waterfallBox.Height);
                var newBmp = new Bitmap(w, h);
                using (var g = Graphics.FromImage(newBmp))
                    g.Clear(Color.Black);

                lock (lockObj)
                {
                    waterfallBitmap?.Dispose();
                    waterfallBitmap = newBmp;
                }

                DrawFrequencyScale();
            }
            finally
            {
                readTimer?.Start();
            }
        }


        private void WaterfallBox_MouseMove(object sender, MouseEventArgs e)
            {
                float freqPerPixel = (FREQ_MAX - FREQ_MIN) / waterfallBox.Width;
                float freq = FREQ_MIN + e.X * freqPerPixel;
                freqLabel.Text = $"{freq:F1} Hz";
            }

            private void StopTimer()
            {
                readTimer?.Stop();
                readTimer?.Dispose();
            }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible)
                readTimer?.Start();
            else
                readTimer?.Stop();
        }
        private void ReadTimer_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = "Idle";
            cycleTimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            if (string.IsNullOrEmpty(wavPath) || !File.Exists(wavPath))
            {
                statusLabel.Text = "Waiting for WAV file...";
                return;
            }

            if (wavPath != lastWavPath)
            {
                lastWavPath = wavPath;
                filePosition = 0;
                fftPos = 0;
                fileCompleteTime = DateTime.MinValue;
                currentFileStartTime = DateTime.Now;
                fileCompleteTime = DateTime.MinValue;
                drawTimeOnNextLine = true;
                pendingTimeLabel = GetWsprTime();  // ← use rounded time
                pendingTimeLabel += " started on:  " + freq + " ^^";
                DrawCycleLine();
                statusLabel.Text = "New file: " + Path.GetFileName(wavPath);
            }

            try
            {
                using (var fs = new FileStream(wavPath, FileMode.Open,
                                               FileAccess.Read, FileShare.ReadWrite))
                {
                    if (filePosition == 0)
                        filePosition = 44;  // skip WAV header

                    long available = fs.Length - filePosition;
                    if (available <= 0)
                    {
                        if (fileCompleteTime == DateTime.MinValue)
                            fileCompleteTime = DateTime.Now;

                        double idleSeconds = (DateTime.Now - fileCompleteTime).TotalSeconds;
                        statusLabel.Text = $"Idle or TX in progress ({(int)idleSeconds}s)";
                        return;
                    }

                    fs.Seek(filePosition, SeekOrigin.Begin);
                    byte[] buffer = new byte[available];
                    int bytesRead = fs.Read(buffer, 0, buffer.Length);
                    filePosition += bytesRead;

                    statusLabel.Text = $"{Path.GetFileName(wavPath)}  {filePosition / 1024}KB";

                    int sampleCount = bytesRead / 2;
                    for (int i = 0; i < sampleCount; i++)
                    {
                        short sample = BitConverter.ToInt16(buffer, i * 2);
                        fftBuffer[fftPos] = sample / 32768f;
                        fftPos++;

                        if (fftPos >= FFT_SIZE)
                        {
                            fftPos = 0;
                            ProcessFFT();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Error: " + ex.Message;
            }
        }

        private void LineTimer_Tick(object sender, EventArgs e)
        {
            if (currentFileStartTime == DateTime.MinValue) return;
            double elapsed = (DateTime.Now - currentFileStartTime).TotalSeconds;
            if (elapsed >= 120)
            {
                DrawCycleLine();
                currentFileStartTime = DateTime.Now;  // reset for next cycle
            }
        }

        private void DrawCycleLineBottom()
        {
            lock (lockObj)
            {
                if (waterfallBitmap == null) return;
                using (var g = Graphics.FromImage(waterfallBitmap))
                {
                    // Draw a bright white horizontal line
                    g.DrawLine(new Pen(Color.White, 2),
                              0, waterfallBitmap.Height - 1,
                              waterfallBitmap.Width, waterfallBitmap.Height - 1);
                }
                waterfallBox.Image = waterfallBitmap;
            }
        }

        private void DrawCycleLine()
        {
            lock (lockObj)
            {
                if (waterfallBitmap == null) return;
                using (var g = Graphics.FromImage(waterfallBitmap))
                {
                    // Fill top data line white
                    g.DrawLine(new Pen(Color.White, 4), 0, 22, waterfallBitmap.Width, 22);
                }
                waterfallBox.Image = waterfallBitmap;
            }
        }

        private void ProcessFFT()
            {
                
            // Apply Hann window
            Complex[] complexData = new Complex[FFT_SIZE];
                for (int i = 0; i < FFT_SIZE; i++)
                {
                    double window = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (FFT_SIZE - 1)));
                    complexData[i].X = (float)(fftBuffer[i] * window);
                    complexData[i].Y = 0;
                }

                FastFourierTransform.FFT(true, (int)Math.Log(FFT_SIZE, 2), complexData);

                // Extract WSPR frequency range only
                float freqResolution = (float)SAMPLE_RATE / FFT_SIZE;
                int binMin = (int)(FREQ_MIN / freqResolution);
                int binMax = (int)(FREQ_MAX / freqResolution);
                int binCount = binMax - binMin;

                float[] magnitudes = new float[binCount];
               
                for (int i = 0; i < binCount; i++)
                {
                    var c = complexData[binMin + i];
                    float mag = (float)Math.Sqrt(c.X * c.X + c.Y * c.Y);
                    magnitudes[i] = (float)(20 * Math.Log10(mag + 1e-6)) + gainDb;
                }

                if (!this.IsDisposed)
                    this.Invoke((Action)(() => DrawWaterfallLineDown(magnitudes)));
            }

        private void DrawWaterfallLineUp(float[] magnitudes)
        {
            lock (lockObj)
            {
                if (waterfallBitmap == null) return;
                int w = waterfallBitmap.Width;
                int h = waterfallBitmap.Height;

                // Scroll up by 1 pixel
                Rectangle srcRect = new Rectangle(0, 1, w, h - 1);
                Rectangle dstRect = new Rectangle(0, 0, w, h - 1);
                using (var g = Graphics.FromImage(waterfallBitmap))
                    g.DrawImage(waterfallBitmap, dstRect, srcRect, GraphicsUnit.Pixel);

                // Draw new line at bottom
                for (int x = 0; x < w; x++)
                {
                    int magIndex = (int)((float)x / w * magnitudes.Length);
                    magIndex = Math.Max(0, Math.Min(magnitudes.Length - 1, magIndex));
                    float db = magnitudes[magIndex];
                    float norm = Math.Max(0, Math.Min(1, (db + 80) / 60f));
                    waterfallBitmap.SetPixel(x, h - 1, MagnitudeToColor(norm));
                }

                // Draw time label on first line of new recording
                if (drawTimeOnNextLine)
                {
                    drawTimeOnNextLine = false;
                    using (var g = Graphics.FromImage(waterfallBitmap))
                    using (var font = new Font("Courier New", 8, FontStyle.Bold))
                    {
                        g.FillRectangle(Brushes.DarkSlateGray, 0, h - 14, 180, 10);
                        g.DrawString(pendingTimeLabel, font, Brushes.Yellow, 2, h - 14);
                    }
                }

                DrawFrequencyScale();
                waterfallBox.Image = waterfallBitmap;
            }
        }

        private void DrawWaterfallLineDown(float[] magnitudes)
        {
            lock (lockObj)
            {
                if (waterfallBitmap == null || waterfallBox.Width < 1 || waterfallBox.Height < 1) return;
                int w = waterfallBitmap.Width;
                int h = waterfallBitmap.Height;

                // Scroll down by 1 pixel
                Rectangle srcRect = new Rectangle(0, 0, w, h - 1);
                Rectangle dstRect = new Rectangle(0, 1, w, h - 1);
                using (var g = Graphics.FromImage(waterfallBitmap))
                    g.DrawImage(waterfallBitmap, dstRect, srcRect, GraphicsUnit.Pixel);

                // Draw new line at bottom
                for (int x = 0; x < w; x++)
                {
                    int magIndex = (int)((float)x / w * magnitudes.Length);
                    magIndex = Math.Max(0, Math.Min(magnitudes.Length - 1, magIndex));
                    float db = magnitudes[magIndex];
                    float norm = Math.Max(0, Math.Min(1, (db + 80) / 60f));
                    waterfallBitmap.SetPixel(x, 22, MagnitudeToColor(norm));
                }

                // Draw time label at top after scroll
                if (drawTimeOnNextLine)
                {
                    drawTimeOnNextLine = false;
                    using (var g = Graphics.FromImage(waterfallBitmap))
                    using (var font = new Font("Courier New", 8, FontStyle.Bold))
                    {
                        g.FillRectangle(Brushes.DarkSlateGray, 0, 22, 205, 12);
                        g.DrawString(pendingTimeLabel, font, Brushes.Yellow, 1, 22);
                    }
                }

                DrawFrequencyScale();
                waterfallBox.Image = waterfallBitmap;
            }
        }
        private string GetWsprTime()
        {
            DateTime now = DateTime.Now;
            int minute = now.Minute;

            if (minute % 2 != 0)
            {
                // Odd minute - round up to next even minute
                minute++;
            }
            // Even minute - keep as is, no change

            if (minute >= 60) minute -= 60;

            return $"{now.Hour:D2}:{minute:D2}";
        }

        private Color MagnitudeToColor(float norm)
            {
                if (norm < 0.2f)
                {
                    float t = norm / 0.2f;
                    return Color.FromArgb(0, 0, (int)(255 * t));
                }
                else if (norm < 0.4f)
                {
                    float t = (norm - 0.2f) / 0.2f;
                    return Color.FromArgb(0, (int)(255 * t), 255);
                }
                else if (norm < 0.6f)
                {
                    float t = (norm - 0.4f) / 0.2f;
                    return Color.FromArgb(0, 255, (int)(255 * (1 - t)));
                }
                else if (norm < 0.8f)
                {
                    float t = (norm - 0.6f) / 0.2f;
                    return Color.FromArgb((int)(255 * t), 255, 0);
                }
                else
                {
                    float t = (norm - 0.8f) / 0.2f;
                    return Color.FromArgb(255, (int)(255 * (1 - t)), 0);
                }
            }
        }
    }
    /*public SpectrumForm()
        {
            InitializeComponent();
        }

        private void SpectrumForm_Load(object sender, EventArgs e)
        {

        }
    }
}*/
