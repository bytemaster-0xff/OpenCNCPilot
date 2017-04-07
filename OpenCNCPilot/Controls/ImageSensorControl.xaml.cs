using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LagoVista.GCode.Sender.Application.Controls
{
    public partial class ImageSensorControl : UserControl
    {

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        bool _timerStopped = true;
        VideoCapture _videoCapture;
        Object _videoCaptureLocker = new object();

        public ImageSensorControl()
        {
            InitializeComponent();
            Stop.Visibility = Visibility.Collapsed;
            DataContextChanged += ImageSensorControl_DataContextChanged;
        }

        private void ImageSensorControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel != null && ViewModel.Machine.IsInitialized)
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

        private bool _running;
        public bool CameraHasData;
      
        private void CaptureLoop()
        {
            Task.Run(async () =>
            {
                while (_running)
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lock (_videoCaptureLocker)
                        {
                            if (_videoCapture != null)
                            {
                                using (var originalFrame = _videoCapture.QueryFrame())
                                {
                                    var result = PerformShapeDetection(null, originalFrame);
                                }

                            }
                        }
                    }));

                    await Task.Delay(100);
                }
            });

            
        }

        private async Task InitCapture(int cameraIndex)
        {
            await Task.Run(() =>
           {
               try
               {
                   _videoCapture = new VideoCapture(cameraIndex);

                   _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                   _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, 33);
                   _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, 54);
                   _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, -7);
               }
               catch (Exception ex)
               {
                   Core.PlatformSupport.Services.Logger.LogException("ImageSensor_InitCapture", ex);
               }
           });
        }

        public async void StartCapture()
        {
            lock (_videoCaptureLocker)
            {
                if (_videoCapture != null)
                {
                    return;
                }
            }

            if (ViewModel.Machine.Settings.PositioningCamera == null)
            {
                MessageBox.Show("Please select a camera");
                new SettingsWindow(ViewModel.Machine, ViewModel.Machine.Settings, 2).ShowDialog();
                return;
            }

            try
            {
                LoadingMask.Visibility = Visibility.Visible;
                await InitCapture(ViewModel.Machine.Settings.PositioningCamera.CameraIndex);

                Play.Visibility = Visibility.Collapsed;
                Stop.Visibility = Visibility.Visible;

                CaptureLoop();

                _timerStopped = false;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
            finally
            {
                LoadingMask.Visibility = Visibility.Collapsed;
            }

        }

        public void StopCapture()
        {

            _running = false;

            lock (_videoCaptureLocker)
            {
                if (_videoCapture != null)
                {
                    _videoCapture.Stop();
                    _videoCapture = null;
                }
            }

            Play.Visibility = Visibility.Visible;
            Stop.Visibility = Visibility.Collapsed;

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
            lock (_videoCaptureLocker)
            {
                if (_videoCapture != null)
                {
                    _videoCapture.Stop();
                    _videoCapture = null;
                }
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(ViewModel.Machine, ViewModel.Machine.Settings, 2).ShowDialog();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            StartCapture();
        }

        MachineVision _fullScreenWindow;

        private void ShowFullScreenVision_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();

            if (_fullScreenWindow != null)
            {
                _fullScreenWindow.BringIntoView();
            }
            else
            {
                _fullScreenWindow = new MachineVision(ViewModel.Machine);
                _fullScreenWindow.Closed += _fullScreenWindow_Closed;
                _fullScreenWindow.Show();
            }
        }

        private void _fullScreenWindow_Closed(object sender, EventArgs e)
        {
            _fullScreenWindow = null;
        }
    }
}