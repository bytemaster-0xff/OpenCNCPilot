using Emgu.CV;
using Emgu.CV.Structure;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LagoVista.GCode.Sender.Application.Controls
{
    public partial class ImageSensorControl : UserControl
    {

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        Timer _timer;
        bool _timerStopped = true;
        VideoCapture _videoCapture;
        Object _videoCaptureLocker = new object();

        public ImageSensorControl()
        {
            InitializeComponent();
            Stop.IsEnabled = true;
            DataContextChanged += ImageSensorControl_DataContextChanged;
        }

        private void ImageSensorControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Machine.Settings.PropertyChanged += Settings_PropertyChanged;
                ViewModel.Machine.PropertyChanged += (sndr, args) =>
                {
                    if (args.PropertyName == nameof(ViewModel.Machine.Settings))
                    {
                        ViewModel.Machine.Settings.PropertyChanged += Settings_PropertyChanged;
                    }
                };
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Machine.Settings.PositioningCamera))
            {
                StopCapture();
                if (ViewModel.Machine.Settings.PositioningCamera != null)
                {
                    StartCapture();
                }
            }
        }

        private void ProcessFrame(Object state)
        {
            if (_timerStopped)
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (_videoCaptureLocker)
                {
                    if (_videoCapture != null)
                    {
                        using (var originalFrame = _videoCapture.QueryFrame())
                        using (var gray = new Image<Gray, byte>(originalFrame.Bitmap))
                        using (var blurredGray = new Mat())
                        using (var finalOutput = new Mat())
                        {
                            CvInvoke.GaussianBlur(gray, blurredGray, new System.Drawing.Size(5, 5), 3);
                            CvInvoke.Canny(blurredGray, finalOutput, 15, 45, 3);
                            var segments = CvInvoke.HoughLinesP(finalOutput, 1, Math.PI / 2, 50, 5, 5);

                            foreach (var segment in segments)
                            {
                                CvInvoke.Line(originalFrame,
                                    segment.P1,
                                    segment.P2,
                                    new MCvScalar(0x00, 0x00, 0xFF), 2, Emgu.CV.CvEnum.LineType.AntiAlias);
                            }

                            WebCamImage.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(originalFrame);
                        }
                    }
                }
            }));
        }

        public void StartCapture()
        {
            lock (_videoCaptureLocker)
            {
                if (_videoCapture != null)
                {
                    return;
                }

                if (ViewModel.Machine.Settings.PositioningCamera == null)
                {
                    MessageBox.Show("Please select a camera");
                    new SettingsWindow(ViewModel.Machine, 2).ShowDialog();
                    return;
                }

                try
                {
                    _videoCapture = new VideoCapture(ViewModel.Machine.Settings.PositioningCamera.CameraIndex);
                    _timer = new Timer(ProcessFrame, _videoCapture, 0, 100);
                    _timerStopped = false;
                    Stop.IsEnabled = true;
                    Play.IsEnabled = false;
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }
        }

        public void StopCapture()
        {
            if (_timer != null)
            {
                _timerStopped = true;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }

            lock (_videoCaptureLocker)
            {
                if (_videoCapture != null)
                {
                    _videoCapture.Dispose();
                    _videoCapture = null;
                }
            }

            Stop.IsEnabled = false;
            Play.IsEnabled = true;

            var src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri("pack://application:,,/Imgs/TestPattern.jpg");
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            WebCamImage.Source = src;
        }

        public MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();
        }

        public void ShutDown()
        {
            StopCapture();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(ViewModel.Machine, 2).ShowDialog();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            StartCapture();
        }
    }
}