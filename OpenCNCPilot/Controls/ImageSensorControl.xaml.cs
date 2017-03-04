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

        Timer _timer;
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

        public bool CameraHasData;
        /*
        private void FinishProcessing(Image<Gray, Byte> cannyEdges, Image<Bgr, Byte> frame)
        {
            List<Triangle2DF> triangleList = new List<Triangle2DF>();
            var boxList = new List<Emgu.CV.Structure.MCvBox2D>(); //a box is a rotated rectangle
            using (var storage = new MemStorage()) //allocate storage for contour approximation
            {
                var rectangles = new List<Rectangle>();

                var contoursDetected = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(cannyEdges, contoursDetected,null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                var contoursArray = new List<VectorOfPoint>();
                int count = contoursDetected.Size;
                for (int i = 0; i < count; i++)
                {
                    CvInvoke.ApproxPolyDP()
                    rectangles.Add(CvInvoke.BoundingRectangle(contoursArray[i]));

                    using (VectorOfPoint currContour = contoursDetected[i])
                    {
                        contoursArray.Add(currContour);
                    }
                }
                
                foreach(var contour in contoursArray)
                { 
                    var currentContour = contour.ApproxPoly(contour.Perimeter * 0.05, storage);

                    if (currentContour.Area > 400 && currentContour.Area < 20000) //only consider contours with area greater than 250
                    {
                        if (currentContour.Total == 4) //The contour has 4 vertices.
                        {
                            // determine if all the angles in the contour are within [80, 100] degree
                            bool isRectangle = true;
                            var pts = currentContour.ToArray();
                            var edges = PointCollection.PolyLine(pts, true);

                            for (int i = 0; i < edges.Length; i++)
                            {
                                double angle = Math.Abs(
                                   edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                                if (angle < 80 || angle > 100)
                                {
                                    isRectangle = false;
                                    break;
                                }
                            }

                            if (isRectangle) boxList.Add(currentContour.GetMinAreaRect());
                        }
                    }
                    
                }
            Image<Bgr, Byte> triangleRectangleImage = frame.CopyBlank();
            foreach (Triangle2DF triangle in triangleList)
                triangleRectangleImage.Draw(triangle, new Bgr(Color.DarkBlue), 2);
            foreach (var box in boxList)
            {*/
        /*
        frm.SetText(frm.Controls["textBoxImageY"], box.center.Y.ToString());
        frm.SetText(frm.Controls["textBoxDeg"], box.angle.ToString());
        frm.SetText(frm.Controls["textBoxImageX"], box.center.X.ToString());
         * */
        /*                CameraHasData = true;

                        triangleRectangleImage.Draw(box, new Bgr(Color.DarkOrange), 2);
                    }
                    // add cross hairs to image
                    int totalwidth = frame.Width;
                    int totalheight = frame.Height;
                    PointF[] linepointshor = new PointF[] {
                            new PointF(0, totalheight/2),
                            new PointF(totalwidth, totalheight/2)

                        };
                    PointF[] linepointsver = new PointF[] {
                            new PointF(totalwidth/2, 0),
                            new PointF(totalwidth/2, totalheight)

                        };
                    triangleRectangleImage.DrawPolyline(Array.ConvertAll<PointF, System.Drawing.Point>(linepointshor, System.Drawing.Point.Round), false, new Bgr(Color.AntiqueWhite), 1);
                    triangleRectangleImage.DrawPolyline(Array.ConvertAll<PointF, System.Drawing.Point>(linepointsver, System.Drawing.Point.Round), false, new Bgr(Color.AntiqueWhite), 1);
                    ImageOverlay.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(triangleRectangleImage);            
                }*/

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

                            CvInvoke.GaussianBlur(gray, blurredGray, new System.Drawing.Size(7, 7), 0);
                            var results = PerformShapeDetection(blurredGray);

                            CvInvoke.Canny(blurredGray, finalOutput, 0, 10, 5, false);
                            CvInvoke.Threshold(finalOutput, finalOutput, 100, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

                            double cannyThreshold = 140.0;
                            double cannyThresholdLinking = 120.0;
                            var cannyEdges = gray.Canny(cannyThreshold, cannyThresholdLinking);

                            var segments = CvInvoke.HoughLinesP(finalOutput, 1, Math.PI / 2, 30, 5, 5);
                            foreach (var segment in results.Lines)
                            {
                                CvInvoke.Line(blurredGray,
                                    segment.P1,
                                    segment.P2,
                                    new Bgr(System.Drawing.Color.White).MCvScalar, 2, Emgu.CV.CvEnum.LineType.AntiAlias);
                            }

                        /*    foreach (var circle in results.Circles)
                            {
                                /*CvInvoke.Circle(blurredGray,
                                     new   System.Drawing.Point((int)circle.Center.X, (int)circle.Center.Y), (int)circle.Radius,
                                     new Bgr(System.Drawing.Color.White).MCvScalar, 2, Emgu.CV.CvEnum.LineType.AntiAlias);*/

                             /*   var pt1 = new System.Drawing.Point(0, (int)circle.Center.Y);
                                var pt2 = new System.Drawing.Point(1024, (int)circle.Center.Y);

                                CvInvoke.Line(blurredGray,
                                   pt1,
                                   pt2,
                                   new Bgr(System.Drawing.Color.White).MCvScalar, 2, Emgu.CV.CvEnum.LineType.AntiAlias);

                                var vpt1 = new System.Drawing.Point((int)circle.Center.X, 0);
                                var vpt2 = new System.Drawing.Point((int)circle.Center.X, 1024);

                                CvInvoke.Line(blurredGray,
                                   vpt1,
                                   vpt2,
                                   new Bgr(System.Drawing.Color.White).MCvScalar, 2, Emgu.CV.CvEnum.LineType.AntiAlias);
                            }

                                    foreach(var box in results.Rects)
                                    {
                                        CvInvoke.Polylines(blurredGray, Array.ConvertAll(box.GetVertices(), System.Drawing.Point.Round), 
                                            true, 
                                            new Bgr(System.Drawing.Color.White).MCvScalar, 2);
                                    }*/

                            WebCamImage.Source = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(blurredGray);
                        }
                    }
                }
            }));
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

                ProcessFrame(null);

                _timer = new Timer(ProcessFrame, _videoCapture, 0, 1000);
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
    }
}