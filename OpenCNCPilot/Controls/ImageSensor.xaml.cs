using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenCNCPilot.Controls
{
    /// <summary>
    /// Interaction logic for ImageSensor.xaml
    /// </summary>
    public partial class ImageSensor : UserControl
    {
        private VideoCapture _capture;
        private bool _captureInProgress;
        public delegate void delUpdateImageCallback(Image<Bgr, Byte> Frame);

        public ImageSensor()
        {
            InitializeComponent();
        }

        Mat _finalOutput = new Mat();

        private void ProcessFrame()
        {
            while (_captureInProgress == true)
            {
                BitmapSource outputBitmap = null;

                using (var frame = _capture.QueryFrame())
                using (var gray = new Image<Gray, byte>(frame.Bitmap))
                using (var output = new Mat())
                using (var cannyOutput = new Mat())
                {
                    CvInvoke.GaussianBlur(gray, output, new System.Drawing.Size(5, 5), 3);
                    CvInvoke.Canny(output, _finalOutput, 5, 15, 3);

                    outputBitmap = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(_finalOutput);


                    Dispatcher.BeginInvoke(new Action(() =>
                        {

                            WebCamImage.Source = outputBitmap;

                        }));
                }

            }
        }


        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);



        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (_capture == null)
            {
                try
                {
                    _capture = new VideoCapture();
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            if (_capture != null)
            {
                Thread thrdWebcam = new Thread(new ThreadStart(ProcessFrame));
                _captureInProgress = true;
                thrdWebcam.Start();


            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _captureInProgress = false;
        }
    }
}
