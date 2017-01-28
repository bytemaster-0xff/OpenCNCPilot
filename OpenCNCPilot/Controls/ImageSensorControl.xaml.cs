using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OpenCNCPilot.Controls
{
    /// <summary>
    /// Interaction logic for ImageSensor.xaml
    /// </summary>
    public partial class ImageSensorControl : UserControl
    {
        Timer _timer;

        public ImageSensorControl()
        {
            InitializeComponent();
        }

        Mat _finalOutput = new Mat();


        private void ProcessFrame(Object state)
        {
            var capture = state as VideoCapture;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //while (_captureInProgress == true)
                using (var frame = capture.QueryFrame())
                using (var gray = new Image<Gray, byte>(frame.Bitmap))
                using (var output = new Mat())
                using (var cannyOutput = new Mat())
                using (var finalOutput = new Mat())
                {
                    CvInvoke.GaussianBlur(gray, output, new System.Drawing.Size(5, 5), 3);
                    CvInvoke.Canny(output, finalOutput, 15, 45, 3);
                    var outputBitmap = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(finalOutput);
                    WebCamImage.Source = outputBitmap;
                }
            }));

        }


        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var capture = new VideoCapture();
                _timer = new Timer(ProcessFrame, capture, 0, 100);
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
        }
    }
}
