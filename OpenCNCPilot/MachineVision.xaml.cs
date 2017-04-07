using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Windows;
using Emgu.CV;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Application.ViewModels;

namespace LagoVista.GCode.Sender.Application
{
    /// <summary>
    /// Interaction logic for MachineVision.xaml
    /// </summary>
    public partial class MachineVision : Window
    {


        System.Threading.Timer _timer;
        bool _timerStopped = true;
        VideoCapture _videoCapture;
        Object _videoCaptureLocker = new object();

        public MachineVision(IMachine machine)
        {
            InitializeComponent();

            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                ViewModel = new MachineVisionViewModel(machine);
                DataContext = ViewModel;
                this.Closing += MachineVision_Closing;
                this.Loaded += MachineVision_Loaded;
                this.DataContextChanged += MachineVision_DataContextChanged;

                JogGrid.DataContext = new MachineControlViewModel(machine);
            }
        }

        private void MachineVision_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCapture();
        }

        private void MachineVision_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = DataContext as MachineVisionViewModel;
        }

        private async void MachineVision_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitAsync();
        }

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

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lock (_videoCaptureLocker)
                        {
                            if (_videoCapture != null)
                            {
                                _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                                if (_lastBrightness != ViewModel.Profile.Brightness)
                                {
                                    _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, ViewModel.Profile.Brightness);
                                    _lastBrightness = ViewModel.Profile.Brightness;
                                }

                                if (_lastContrast != ViewModel.Profile.Contrast)
                                {
                                    _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, ViewModel.Profile.Contrast);
                                    _lastContrast = ViewModel.Profile.Contrast;
                                }

                                if (_lastExposure != ViewModel.Profile.Exposure)
                                {
                                    _videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, ViewModel.Profile.Exposure);
                                    _lastExposure = ViewModel.Profile.Exposure;
                                }

                                using (var originalFrame = _videoCapture.QueryFrame())
                                {
                                    using (var results = ViewModel.PerformShapeDetection(originalFrame))
                                    {

                                        if (LoadingMask.Visibility == Visibility.Visible)
                                        {
                                            LoadingMask.Visibility = Visibility.Collapsed;
                                        }

                                        WebCamImage.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
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

            if (ViewModel.Machine.Settings.PositioningCamera == null)
            {
                MessageBox.Show("Please Select a Camera");
                new SettingsWindow(ViewModel.Machine, ViewModel.Machine.Settings, 2).ShowDialog();
                return;
            }

            try
            {
                LoadingMask.Visibility = Visibility.Visible;
                await InitCapture(ViewModel.Machine.Settings.PositioningCamera.CameraIndex);

                Play.Visibility = Visibility.Collapsed;
                Stop.Visibility = Visibility.Visible;
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
                WebCamImage.Source = src;
                Play.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error Shutting Down Video, please restart the application." + ex.Message);
            }
            finally
            {
                Stop.Visibility = Visibility.Collapsed;
                Stop.IsEnabled = true;
            }
        }


        MachineVisionViewModel _viewModel;

        public MachineVisionViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
            }
        }


        private void Play_Click(object sender, RoutedEventArgs e)
        {
            StartCapture();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Stop.IsEnabled = false;
            StopCapture();

        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowLink_Handler(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
