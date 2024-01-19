using AForge.Imaging.Filters ;
using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Vision.Motion;

namespace camera
{
    public partial class Form1 : Form
    {
        Bitmap frame;
        private Bitmap previousFrame = null;
        VideoCaptureDevice capturedDevice;
        HueModifier hueFilter = new HueModifier(180);
        SaturationCorrection satFilter = new SaturationCorrection(0.5f);
        FilterInfoCollection videoDeviceList;
        long zoom = 1;
        bool isConnected = false;
        MotionDetector motionDetector;
        public Form1()
        {
            InitializeComponent();
            videoDeviceList = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo videoDevice in videoDeviceList)
            {
                comboBox1.Items.Add(videoDevice.Name);
            }
            capturedDevice = new VideoCaptureDevice();
            comboBox1.SelectedIndex = 0;
            motionDetector = new MotionDetector(new TwoFramesDifferenceDetector(), new MotionAreaHighlighting());
            Task.Run(() => motionDetected());
        }


        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            frame = (Bitmap)eventArgs.Frame.Clone();
            frame = new Bitmap(frame, new Size((int)(frame.Width * zoom), (int)(frame.Height * zoom)));
            frame = hueFilter.Apply(frame);
            frame = satFilter.Apply(frame);
            previousFrame = (Bitmap)frame.Clone();
            pictureBox1.Image = frame;
            
        }

        private void onButtonStartClick(object sender, EventArgs e)
        {
            if(!isConnected)
            {
                capturedDevice = new VideoCaptureDevice(videoDeviceList[comboBox1.SelectedIndex].MonikerString);
                capturedDevice.NewFrame += new NewFrameEventHandler(video_NewFrame);
                capturedDevice.Start();
                buttonStart.Text = "Rozłącz z kamerą";
            }
            else
            {
                capturedDevice.SignalToStop();
                buttonStart.Text = "Połącz z kamerą";
                pictureBox1.Image = null;
            }
            comboBox1.Visible = isConnected;
            isConnected = !isConnected;
            pictureBox1.Visible = isConnected;
        }

        private void onButtonSaveClick(object sender, EventArgs e)
        {
            if(isConnected)
            {
                SaveFileDialog sfdImage = new SaveFileDialog(); // obiekt pozwalający na wybór lokalizacji i nazwy pliku do zapisania
                sfdImage.Filter = "(*.jpg)|*.jpg";
                Image imageToSave= (Image) pictureBox1.Image.Clone();
                if (sfdImage.ShowDialog() == DialogResult.OK)
                {
                    imageToSave.Save(sfdImage.FileName, System.Drawing.Imaging.ImageFormat.Bmp); // zapisanie obrazu do wybranego pliku
                    MessageBox.Show("Zapisano");
                }
                else 
                    MessageBox.Show("Zapisywanie nie powiodło się");
            }
            else
            {
                MessageBox.Show("Żadne urządzenie nie zostało podłączone, nie można zrobić zdjęcia");
            }  
        }

        public void motionDetected()
        {
            while (true)
            {
                if (isConnected && previousFrame!=null)
                {
                   float f = motionDetector.ProcessFrame(previousFrame);
                    if(f>0)
                    {
                        MessageBox.Show("Wykryto ruch!");
                    }

                }
                System.Threading.Thread.Sleep(1000);
            }
            
        }

        private void trackBarZoom_Scroll(object sender, EventArgs e)
        {
            zoom = trackBarZoom.Value;
        }

        private void trackBarHue_Scroll(object sender, EventArgs e)
        {
            hueFilter= new HueModifier(trackBarHue.Value);
        }

        private void trackBarSat_Scroll(object sender, EventArgs e)
        {
            satFilter = new SaturationCorrection(trackBarSat.Value/100);
        }

    }
}
