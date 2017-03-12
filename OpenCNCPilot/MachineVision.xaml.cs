using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Windows.Input;
using LagoVista.Core.WPF.PlatformSupport;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using LagoVista.GCode.Sender.Interfaces;

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
            }
        }

        public Result PerformShapeDetection(MachineVisionViewModel vm, Mat img)
        {
            using (var gray = new Image<Gray, byte>(img.Bitmap))
            using (var blurredGray = new Mat())
            using (var finalOutput = new Mat())
            {

                //K Must always be odd.
                var k = vm.GaussianKSize;
                if (k % 1 == 0)
                {
                    k += 1;
                }

                CvInvoke.GaussianBlur(gray, blurredGray, new System.Drawing.Size(vm.GaussianKSize, vm.GaussianKSize), vm.GaussianSigmaX);
                WebCamImage.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(blurredGray);

                //Convert the image to grayscale and filter out the noise
                //UMat uimage = new UMat();
                //CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

                //use image pyr to remove noise
                UMat pyrDown = new UMat();
                CvInvoke.PyrDown(blurredGray, pyrDown);
                CvInvoke.PyrUp(pyrDown, blurredGray);

                //Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

                var circles = CvInvoke.HoughCircles(blurredGray, HoughType.Gradient, vm.HoughCirclesDP, vm.HoughCirclesMinDistance, 30, 550);//, vm.HoughCirclesParam1, vm.HoughCirclesParam2, vm.HoughCirclesMinRadius, vm.HoughCirclesMaxRadius);

                UMat cannyEdges = new UMat();
                //CvInvoke.Canny(img, cannyEdges, vm.CannyLowThreshold, vm.CannyHighThreshold, vm.CannyApetureSize, vm.CannyGradient);

                List<Triangle2DF> triangleList = new List<Triangle2DF>();
                List<RotatedRect> boxList = new List<RotatedRect>(); //a box is a rotated rectangle
                //var lines = CvInvoke.HoughLinesP(cannyEdges, vm.HoughLinesRHO, vm.HoughLinesTheta, vm.HoughLinesThreshold, vm.HoughLinesMinLineLength, vm.HoughLinesMaxLineGap);

                /*
                

                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        using (VectorOfPoint contour = contours[i])
                        using (VectorOfPoint approxContour = new VectorOfPoint())
                        {
                            CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                            if (CvInvoke.ContourArea(approxContour, false) > 250) //only consider contours with area greater than 250
                            {
                                if (approxContour.Size == 3) //The contour has 3 vertices, it is a triangle
                                {
                                    var pts = approxContour.ToArray();
                                    triangleList.Add(new Triangle2DF(
                                       pts[0],
                                       pts[1],
                                       pts[2]
                                       ));
                                }
                                else if (approxContour.Size == 4) //The contour has 4 vertices.
                                {
                                    #region determine if all the angles in the contour are within [80, 100] degree
                                    bool isRectangle = true;
                                    var pts = approxContour.ToArray();
                                    LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                                    for (int j = 0; j < edges.Length; j++)
                                    {
                                        double angle = Math.Abs(
                                           edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                        if (angle < 80 || angle > 100)
                                        {
                                            isRectangle = false;
                                            break;
                                        }
                                    }
                                    #endregion

                                    if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
                                }
                            }
                        }
                    }
                }*/


                var results = new Result();
                results.Triangles = triangleList;
                results.Rects = boxList;
                results.Circles = circles.ToList();
                //results.Lines = lines.ToList();

                return results;
            }
        }

        public class Result
        {
            public List<LineSegment2D> Lines { get; set; }
            public List<CircleF> Circles { get; set; }

            public List<Triangle2DF> Triangles { get; set; }

            public List<RotatedRect> Rects { get; set; }
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
                        {

                            var results = PerformShapeDetection(ViewModel, originalFrame);

                            foreach (var circle in results.Circles)
                            {
                                CvInvoke.Circle(originalFrame,
                                     new   System.Drawing.Point((int)circle.Center.X, (int)circle.Center.Y), (int)circle.Radius,
                                     new Bgr(System.Drawing.Color.White).MCvScalar, 2, Emgu.CV.CvEnum.LineType.AntiAlias);

                                var pt1 = new System.Drawing.Point(0, (int)circle.Center.Y);
                                var pt2 = new System.Drawing.Point(1024, (int)circle.Center.Y);

                                CvInvoke.Line(originalFrame,
                                   pt1,
                                   pt2,
                                   new Bgr(System.Drawing.Color.White).MCvScalar, 2, Emgu.CV.CvEnum.LineType.AntiAlias);

                                var vpt1 = new System.Drawing.Point((int)circle.Center.X, 0);
                                var vpt2 = new System.Drawing.Point((int)circle.Center.X, 1024);

                                CvInvoke.Line(originalFrame,
                                   vpt1,
                                   vpt2,
                                   new Bgr(System.Drawing.Color.White).MCvScalar, 2, Emgu.CV.CvEnum.LineType.AntiAlias);
                            }


                        }

                    }
                }
            }));
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

                ProcessFrame(null);

                _timer = new System.Threading.Timer(ProcessFrame, _videoCapture, 0, 100);
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


        public MachineVisionViewModel ViewModel
        {
            get { return DataContext as MachineVisionViewModel; }
            set
            {
                DataContext = value;
            }
        }


        private void Play_Click(object sender, RoutedEventArgs e)
        {
            StartCapture();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowLink_Handler(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
