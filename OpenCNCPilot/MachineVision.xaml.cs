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
using LagoVista.GCode.Sender.Models;
using System.Diagnostics;

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
                this.Loaded += MachineVision_Loaded;
            }
        }

        private async void MachineVision_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitAsync();
        }

        private void Circle(IInputOutputArray img, int x, int y, int radius, System.Drawing.Color color, int thickness = 1)
        {
            CvInvoke.Circle(img,
            new System.Drawing.Point(x, y), radius,
            new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);

        }

        private void Line(IInputOutputArray img, int x1, int y1, int x2, int y2, System.Drawing.Color color, int thickness = 1)
        {
            CvInvoke.Line(img, new System.Drawing.Point(x1, y1),
                new System.Drawing.Point(x2, y2),
                new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
        }


        public Result PerformShapeDetection(VisionProfile vm, Mat img)
        {
            using (var correctedImage = new Mat())
            {

                //CvInvoke.EqualizeHist(img, correctedImage);

                using (var gray = new Image<Gray, byte>(img.Bitmap))
                using (var blurredGray = new Image<Gray, float>(gray.Size))
                using (var finalOutput = new Mat())
                {

                    var whiteColor = new Bgr(System.Drawing.Color.White).MCvScalar;

                    CvInvoke.GaussianBlur(gray, blurredGray, System.Drawing.Size.Empty, vm.GaussianSigmaX);

                    UMat pyrDown = new UMat();
                    CvInvoke.PyrDown(blurredGray, pyrDown);
                    CvInvoke.PyrUp(pyrDown, blurredGray);

                    var destImage = ViewModel.ShowOriginalImage ? img : (IInputOutputArray)blurredGray;

                    UMat edges = new UMat();

                    if (vm.UseCannyEdgeDetection)
                    {
                        CvInvoke.Canny(blurredGray, edges, vm.CannyLowThreshold, vm.CannyHighThreshold, vm.CannyApetureSize, vm.CannyGradient);
                    }
                    else
                    {
                        CvInvoke.Threshold(blurredGray, edges, vm.ThresholdEdgeDetection, 255, ThresholdType.Binary);
                    }

                    if (ViewModel.ShowCircles)
                    {
                        var circles = CvInvoke.HoughCircles(blurredGray, HoughType.Gradient, vm.HoughCirclesDP, vm.HoughCirclesMinDistance, vm.HoughCirclesParam1, vm.HoughCirclesParam2, vm.HoughCirclesMinRadius, vm.HoughCirclesMaxRadius);

                        foreach (var circle in circles)
                        {
                            if (circle.Center.X > ((img.Size.Width / 2) - vm.TargetImageRadius) && circle.Center.X < ((img.Size.Width / 2) + vm.TargetImageRadius) &&
                               circle.Center.Y > ((img.Size.Height / 2) - vm.TargetImageRadius) && circle.Center.Y < ((img.Size.Height / 2) + vm.TargetImageRadius))
                            {
                                Line(destImage, 0, (int)circle.Center.Y, img.Width, (int)circle.Center.Y, ViewModel.ShowOriginalImage ? System.Drawing.Color.Red : System.Drawing.Color.White);
                                Line(destImage, (int)circle.Center.X, 0, (int)circle.Center.X, img.Size.Height, ViewModel.ShowOriginalImage ? System.Drawing.Color.Red : System.Drawing.Color.White);

                                CenterX.Text = $"{circle.Center.X:0.0}";
                                CenterY.Text = $"{circle.Center.Y:0.0}";
                            }
                        }
                    }

                    if (ViewModel.ShowCrossHairs)
                    {
                        var width = img.Size.Width;
                        var height = img.Size.Height;

                        var centerX = img.Size.Width / 2;
                        var centerY = img.Size.Height / 2;

                        Circle(destImage, centerX, centerY, vm.TargetImageRadius, ViewModel.ShowOriginalImage ? System.Drawing.Color.Yellow : System.Drawing.Color.White);

                        Line(destImage, 0, centerY, centerX - vm.TargetImageRadius, centerY, ViewModel.ShowOriginalImage ? System.Drawing.Color.Yellow : System.Drawing.Color.White);
                        Line(destImage, centerX + vm.TargetImageRadius, centerY, width, centerY, ViewModel.ShowOriginalImage ? System.Drawing.Color.Yellow : System.Drawing.Color.White);

                        Line(destImage, centerX, 0, centerX, centerY - vm.TargetImageRadius, ViewModel.ShowOriginalImage ? System.Drawing.Color.Yellow : System.Drawing.Color.White);
                        Line(destImage, centerX, centerY + vm.TargetImageRadius, centerX, height, ViewModel.ShowOriginalImage ? System.Drawing.Color.Yellow : System.Drawing.Color.White);
                    }


                    if (ViewModel.ShowLines)
                    {
                        var lines = CvInvoke.HoughLinesP(edges, vm.HoughLinesRHO, vm.HoughLinesTheta * (Math.PI / 180), vm.HoughLinesThreshold, vm.HoughLinesMinLineLength, vm.HoughLinesMaxLineGap);
                        foreach (var line in lines)
                        {
                            CvInvoke.Line(destImage, line.P1, line.P2, new Bgr(System.Drawing.Color.White).MCvScalar);
                        }
                    }

                    if (ViewModel.ShowHarrisCorners)
                    {

                        using (var cornerDest = new Image<Gray, float>(blurredGray.Size))
                        using (var matNormalized = new Image<Gray, float>(blurredGray.Size))
                        using (var matScaled = new Image<Gray, float>(blurredGray.Size))
                        {
                            cornerDest.SetZero();

                            int max = -1;
                            int x = -1, y = -1;

                            CvInvoke.CornerHarris(blurredGray, cornerDest, vm.HarrisCornerBlockSize, vm.HarrisCornerAperture, vm.HarrisCornerK, BorderType.Default);

                            CvInvoke.Normalize(cornerDest, matNormalized, 0, 255, NormType.MinMax, DepthType.Cv32F);
                            CvInvoke.ConvertScaleAbs(matNormalized, matScaled, 10, 5);
                            var minX = (img.Size.Width / 2) - vm.TargetImageRadius;
                            var maxX = (img.Size.Width / 2) + vm.TargetImageRadius;
                            var minY = (img.Size.Height / 2) - vm.TargetImageRadius;
                            var maxY = (img.Size.Height / 2) + vm.TargetImageRadius;

                            for (int j = minX; j < maxX; j++)
                            {
                                for (int i = minY; i < maxY; i++)
                                {
                                    var value = (int)matNormalized.Data[i, j, 0];
                                    if (value > max)
                                    {
                                        x = j;
                                        y = i;
                                        max = value;
                                    }
                                }
                            }

                            Circle(destImage, x, y, 5, System.Drawing.Color.Blue);
                            Line(destImage, 0, y, img.Width, y, System.Drawing.Color.Blue);
                            Line(destImage, x, 0, x, img.Height, System.Drawing.Color.Blue);
                        }
                    }


                    var triangleList = new List<Triangle2DF>();
                    var boxList = new List<RotatedRect>(); //a box is a rotated rectangle

                    if (ViewModel.ShowRectangles)
                    {
                        using (var contours = new VectorOfVectorOfPoint())
                        {
                            CvInvoke.FindContours(edges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                            int count = contours.Size;
                            for (int i = 0; i < count; i++)
                            {
                                using (var contour = contours[i])
                                using (var approxContour = new VectorOfPoint())
                                {
                                    CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * vm.PolygonEpsilonFactor, vm.ContourFindOnlyClosed);
                                    if (CvInvoke.ContourArea(approxContour, false) > vm.ContourMinArea) //only consider contours with area greater than 250
                                    {
                                        var pts = approxContour.ToArray();

                                        if (approxContour.Size == 4) //The contour has 4 vertices.
                                        {
                                            bool isRectangle = true;
                                            var rectEdges = PointCollection.PolyLine(pts, true);

                                            if (!vm.FindIrregularPolygons)
                                            {
                                                for (var j = 0; j < rectEdges.Length; j++)
                                                {
                                                    var angle = Math.Abs(rectEdges[(j + 1) % rectEdges.Length].GetExteriorAngleDegree(rectEdges[j]));
                                                    if (angle < 80 || angle > 100)
                                                    {
                                                        isRectangle = false;
                                                        break;
                                                    }
                                                }
                                            }

                                            if (isRectangle)
                                            {
                                                var rect = CvInvoke.MinAreaRect(approxContour);

                                                var point1 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));
                                                var point2 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                                var point3 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                                var point4 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));

                                                CvInvoke.Line(destImage, point1, point2, new Bgr(System.Drawing.Color.White).MCvScalar);
                                                CvInvoke.Line(destImage, point2, point3, new Bgr(System.Drawing.Color.White).MCvScalar);
                                                CvInvoke.Line(destImage, point3, point4, new Bgr(System.Drawing.Color.White).MCvScalar);
                                                CvInvoke.Line(destImage, point4, point1, new Bgr(System.Drawing.Color.White).MCvScalar);
                                            }
                                        }
                                        else
                                        {
                                            var rectEdges = PointCollection.PolyLine(pts, true);
                                            for (var idx = 0; idx < rectEdges.Length - 1; ++idx)
                                            {
                                                CvInvoke.Line(destImage, rectEdges[idx].P1, rectEdges[idx].P2, new Bgr(System.Drawing.Color.White).MCvScalar);
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }

                    WebCamImage.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(destImage as Mat);


                    var results = new Result();
                    //results.Triangles = triangleList;
                    //results.Rects = boxList;
                    //results.Circles = circles.ToList();
                    //results.Lines = lines.ToList();

                    return results;
                }
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

                }
                catch (Exception ex)
                {
                    Core.PlatformSupport.Services.Logger.LogException("ImageSensor_InitCapture", ex);
                }
            });
        }

        private double _lastContrast = -9999;
        private double _lastBrightness = -9999;
        private double _lastExposure = -9999;

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

                            var results = PerformShapeDetection(ViewModel.Profile, originalFrame);
                            /*
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
                            */

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
