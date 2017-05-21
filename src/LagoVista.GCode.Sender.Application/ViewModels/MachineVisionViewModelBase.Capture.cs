﻿using Emgu.CV;
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
        VideoCapture _topCameraCapture;
        VideoCapture _bottomCameraCapture;

        Object _videoCaptureLocker = new object();

        private VideoCapture InitCapture(int cameraIndex)
        {
            try
            {
                return new VideoCapture(cameraIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open camera: " + ex.Message);
                return null;
            }
        }

        private bool _running;

        private double _lastTopBrightness = -9999;
        private double _lastBottomBrightness = -9999;


        private double _lastTopFocus = -9999;
        private double _lastBottomFocus = -9999;

        private double _lastTopExposure = -9999;
        private double _lastBottomExposure = -9999;

        private double _lastTopContrast = -9999;
        private double _lastBottomContrast = -9999;

        private async void StartImageRecognization()
        {
            _running = true;

            while (_running)
            {
                if (_topCameraCapture != null)
                {
                    _topCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                    if (_lastTopBrightness != _topCameraProfile.Brightness)
                    {
                        _topCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, _topCameraProfile.Brightness);
                        _lastTopBrightness = _topCameraProfile.Brightness;
                    }

                    if (_lastTopFocus != _topCameraProfile.Focus)
                    {
                        _topCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, _topCameraProfile.Focus);
                        _lastTopFocus = _topCameraProfile.Focus;
                    }

                    if (_lastTopContrast != _topCameraProfile.Contrast)
                    {
                        _topCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, _topCameraProfile.Contrast);
                        _lastTopContrast = _topCameraProfile.Contrast;
                    }

                    if (_lastTopExposure != _topCameraProfile.Exposure)
                    {
                        _topCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, _topCameraProfile.Exposure);
                        _lastTopExposure = _topCameraProfile.Exposure;
                    }

                    HasFrame = true;

                    if (LoadingMask)
                    {
                        LoadingMask = false;
                    }

                    using (var originalFrame = _topCameraCapture.QueryFrame())
                    using (var results = PerformShapeDetection(originalFrame))
                    {
                        if (ShowTopCamera)
                        {
                            PrimaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                        }
                        else if (PictureInPicture)
                        {
                            SecondaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                        }
                    }
                }

                if (_bottomCameraCapture != null)
                {
                    _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                    if (_lastBottomBrightness != _bottomCameraProfile.Brightness)
                    {
                        _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, _bottomCameraProfile.Brightness);
                        _lastBottomBrightness = _bottomCameraProfile.Brightness;
                    }

                    if (_lastBottomFocus != _bottomCameraProfile.Focus)
                    {
                        _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, _bottomCameraProfile.Focus);
                        _lastBottomFocus = _bottomCameraProfile.Focus;
                    }

                    if (_lastBottomContrast != _bottomCameraProfile.Contrast)
                    {
                        _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, _bottomCameraProfile.Contrast);
                        _lastBottomContrast = _bottomCameraProfile.Contrast;
                    }

                    if (_lastBottomExposure != _bottomCameraProfile.Exposure)
                    {
                        _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, _bottomCameraProfile.Exposure);
                        _lastBottomExposure = _bottomCameraProfile.Exposure;
                    }

                    using (var originalFrame = _bottomCameraCapture.QueryFrame())
                    using (var results = PerformShapeDetection(originalFrame))
                    {
                        if (ShowBottomCamera)
                        {
                            PrimaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                        }
                        else if (PictureInPicture)
                        {
                            SecondaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                        }
                    }
                    HasFrame = true;

                    if (LoadingMask)
                    {
                        LoadingMask = false;
                    }

                    if (!ShowTopCamera && !ShowBottomCamera)
                    {
                        PrimaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
                        SecondaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
                    }
                }

                await Task.Delay(100);
            }


            HasFrame = false;
        }

        public void StartCapture()
        {
            if (_topCameraCapture != null)
            {
                return;
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
                if (Machine.Settings.PositioningCamera != null)
                {
                    _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.CameraIndex);
                }
                else
                {
                    _topCameraCapture = null;
                }

                if (Machine.Settings.PartInspectionCamera != null)
                {
                    _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.CameraIndex);
                }
                else
                {
                    _bottomCameraCapture = null;
                }

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
                    if (_topCameraCapture != null)
                    {
                        _topCameraCapture.Stop();
                        _topCameraCapture.Dispose();
                        _topCameraCapture = null;
                    }
                }

                var src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri("pack://application:,,/Imgs/TestPattern.jpg");
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                PrimaryCapturedImage = src;
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
