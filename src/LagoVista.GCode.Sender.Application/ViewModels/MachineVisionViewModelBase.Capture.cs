using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public abstract partial class MachineVisionViewModelBase
    {
        System.Threading.Timer _timer;
        bool _timerStopped = true;
        VideoCapture _videoCapture;
        Object _videoCaptureLocker = new object();


        private async Task InitCapture(int cameraIndex)
        {
            await Task.Run(() =>
            {
                try
                {
                    _videoCapture = new VideoCapture(cameraIndex);

                }
                catch (Exception ex)
                {
                    Core.PlatformSupport.Services.Logger.LogException("ImageSensor_InitCapture", ex);
                }
            });
        }

        private bool _running;

        private double _lastContrast = -9999;
        private double _lastBrightness = -9999;
        private double _lastExposure = -9999;


        private void StartImageRecognization()
        {
            Task.Run(async () =>
            {
                while (_running)
                {

                    await Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        lock (_videoCaptureLocker)
                        {
                            if (_videoCapture != null)
                            {
                                _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                                if (_lastBrightness != Profile.Brightness)
                                {
                                    _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, Profile.Brightness);
                                    _lastBrightness = Profile.Brightness;
                                }

                                if (_lastContrast != Profile.Contrast)
                                {
                                    _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, Profile.Contrast);
                                    _lastContrast = Profile.Contrast;
                                }

                                if (_lastExposure != Profile.Exposure)
                                {
                                    _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, Profile.Exposure);
                                    _lastExposure = Profile.Exposure;
                                }

                                using (var originalFrame = _videoCapture.QueryFrame())
                                {
                                    using (var results = PerformShapeDetection(originalFrame))
                                    {

                                        if (LoadingMask)
                                        {
                                            LoadingMask = false;
                                        }

                                        CapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                                    }
                                }
                            }
                        }
                    }));

                    await Task.Delay(100);
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

            if (Machine.Settings.PositioningCamera == null)
            {
                MessageBox.Show("Please Select a Camera");
                new SettingsWindow(Machine, Machine.Settings, 2).ShowDialog();
                return;
            }

            try
            {
                LoadingMask = true;
                await InitCapture(Machine.Settings.PositioningCamera.CameraIndex);
                _running = true;
                StartImageRecognization();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start video, please restart your application: " + ex.Message);
            }

        }

        public void StopCapture()
        {
            try
            {
                _running = false;

                lock (_videoCaptureLocker)
                {
                    if (_videoCapture != null)
                    {
                        _videoCapture.Stop();
                        _videoCapture.Dispose();
                        _videoCapture = null;
                    }
                }

                var src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri("pack://application:,,/Imgs/TestPattern.jpg");
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                CapturedImage = src;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Shutting Down Video, please restart the application." + ex.Message);
            }
            finally
            {
            }
        }

    }
}
