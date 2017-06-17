using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Models;
using LagoVista.GCode.Sender.Util;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public abstract partial class MachineVisionViewModelBase : GCodeAppViewModelBase
    {
        FloatMedianFilter _cornerMedianFilter = new FloatMedianFilter(12, 3);
        FloatMedianFilter _circleMedianFilter = new FloatMedianFilter(12, 3);
        FloatMedianFilter _circleRadiusMedianFilter = new FloatMedianFilter(12, 3);

        private VisionProfile _topCameraProfile;
        private VisionProfile _bottomCameraProfile;


        public MachineVisionViewModelBase(IMachine machine) : base(machine)
        {
            MachineControls = new Sender.ViewModels.MachineControlViewModel(this.Machine);
            Profile = new Models.VisionProfile();
            SaveProfileCommand = new RelayCommand(SaveProfile);

            StartCaptureCommand = new RelayCommand(StartCapture, CanPlay);
            StopCaptureCommand = new RelayCommand(StopCapture, CanStop);
        }

        public override async Task InitAsync()
        {
            _topCameraProfile = await Storage.GetAsync<Models.VisionProfile>("TopCameraVision.json");
            _bottomCameraProfile = await Storage.GetAsync<Models.VisionProfile>("BottomCameraVision.json");

            if (_topCameraProfile == null)
            {
                _topCameraProfile = new VisionProfile();
            }

            if (_bottomCameraProfile == null)
            {
                _bottomCameraProfile = new VisionProfile();
            }

            if (ShowTopCamera)
            {
                Profile = _topCameraProfile;
            }

            if (ShowBottomCamera)
            {
                Profile = _bottomCameraProfile;
            }
        }

        public async void SaveProfile()
        {
            await Storage.StoreAsync(_topCameraProfile, "TopCameraVision.json");
            await Storage.StoreAsync(_bottomCameraProfile, "BottomCameraVision.json");
        }

        public RelayCommand SaveProfileCommand { get; private set; }


        protected void Circle(IInputOutputArray img, int x, int y, int radius, System.Drawing.Color color, int thickness = 1)
        {
            if (!ShowOriginalImage)
            {
                color = System.Drawing.Color.White;
            }

            CvInvoke.Circle(img,
            new System.Drawing.Point(x, y), radius,
            new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);

        }

        protected void Line(IInputOutputArray img, int x1, int y1, int x2, int y2, System.Drawing.Color color, int thickness = 1)
        {
            if (!ShowOriginalImage)
            {
                color = System.Drawing.Color.White;
            }

            CvInvoke.Line(img, new System.Drawing.Point(x1, y1),
                new System.Drawing.Point(x2, y2),
                new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
        }

        private Point2D<double> _foundCorner;
        public Point2D<double> FoundCorner
        {
            get { return _foundCorner; }
            set { Set(ref _foundCorner, value); }
        }

        public virtual void RectLocated(RotatedRect rect, Point2D<double> stdDeviation) { }
        public virtual void RectCentered(RotatedRect rect, Point2D<double> stdDeviation) { }


        public virtual void CornerLocated(Point2D<double> point, Point2D<double> stdDeviation) { }
        public virtual void CornerCentered(Point2D<double> point, Point2D<double> stdDeviation) { }


        public virtual void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDeviation) { }
        public virtual void CircleCentered(Point2D<double> point, double diameter) { }

        #region Show Cross Hairs
        private void DrawCrossHairs(IInputOutputArray destImage, Size size)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            Circle(destImage, center.X, center.Y, Profile.TargetImageRadius, System.Drawing.Color.Yellow);

            Line(destImage, 0, center.Y, center.X - Profile.TargetImageRadius, center.Y, System.Drawing.Color.Yellow);
            Line(destImage, center.X + Profile.TargetImageRadius, center.Y, size.Width, center.Y, System.Drawing.Color.Yellow);

            Line(destImage, center.X, 0, center.X, center.Y - Profile.TargetImageRadius, System.Drawing.Color.Yellow);
            Line(destImage, center.X, center.Y + Profile.TargetImageRadius, center.X, size.Height, System.Drawing.Color.Yellow);

            Line(destImage, center.X - Profile.TargetImageRadius, center.Y, center.X + Profile.TargetImageRadius, center.Y, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));
            Line(destImage, center.X, center.Y - Profile.TargetImageRadius, center.X, center.Y + Profile.TargetImageRadius, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));
        }
        #endregion

        protected Point2D<double> RequestedPosition
        {
            get;
            set;
        }


        protected void JogToLocation(Point2D<double> offset)
        {
            if (StandardDeviation.X < 0.5 && StandardDeviation.Y < 0.5)
            {
                _stabilizedPointCount++;
                if (_stabilizedPointCount > 10)
                {
                    var newLocationX = Math.Round(Machine.NormalizedPosition.X - (offset.X / 20), 4);
                    var newLocationY = Math.Round(Machine.NormalizedPosition.Y + (offset.Y / 20), 4);
                    RequestedPosition = new Point2D<double>() { X = newLocationX, Y = newLocationY };

                    Machine.GotoPoint(RequestedPosition, true);
                    _stabilizedPointCount = 0;
                }
            }
            else
            {
                _stabilizedPointCount = 0;
            }
        }


        int _stabilizedPointCount = 0;

        #region Find Circles
        private void FindCircles(IInputOutputArray input, IInputOutputArray output, Size size)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            var circles = CvInvoke.HoughCircles(input, HoughType.Gradient, Profile.HoughCirclesDP, Profile.HoughCirclesMinDistance, Profile.HoughCirclesParam1, Profile.HoughCirclesParam2, Profile.HoughCirclesMinRadius, Profile.HoughCirclesMaxRadius);

            var foundCircle = false;
            /* Above will return ALL maching circles, we only want the first one that is in the target image radius in the middle of the screen */
            foreach (var circle in circles)
            {
                if (circle.Center.X > ((size.Width / 2) - Profile.TargetImageRadius) && circle.Center.X < ((size.Width / 2) + Profile.TargetImageRadius) &&
                   circle.Center.Y > ((size.Height / 2) - Profile.TargetImageRadius) && circle.Center.Y < ((size.Height / 2) + Profile.TargetImageRadius))
                {
                    _circleMedianFilter.Add(circle.Center.X, circle.Center.Y);
                    _circleRadiusMedianFilter.Add(circle.Radius, 0);
                    foundCircle = true;
                    break;
                }
            }

            if (!foundCircle)
            {
                _circleMedianFilter.Add(null);
                _circleRadiusMedianFilter.Add(null);
            }

            var avg = _circleMedianFilter.Filtered;
            if (avg != null)
            {
                CircleCenter = new Point2D<double>(Math.Round(avg.X, 2), Math.Round(avg.Y, 2));
                StandardDeviation = _circleMedianFilter.StandardDeviation;

                var offset = new Point2D<double>(center.X - avg.X, center.Y - avg.Y);

                /* If within one pixel of center, state we have a match */
                if ((avg.X >= ((size.Width / 2) - 1)) && avg.X <= ((size.Width / 2) + 1) &&
                   (avg.Y >= ((size.Height / 2) - 1)) && avg.Y <= ((size.Height / 2) + 1))
                {
                    Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Green);
                    Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Green);
                    Circle(output, (int)avg.X, (int)avg.Y, (int)_circleRadiusMedianFilter.Filtered.X, System.Drawing.Color.Green);
                    if (StandardDeviation.X < 0.5 && StandardDeviation.Y < 0.5)
                    {
                        _stabilizedPointCount++;
                        if (_stabilizedPointCount > 10)
                        {
                            CircleCentered(offset, _circleRadiusMedianFilter.Filtered.X);
                        }
                    }
                }
                else
                {
                    Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Red);
                    Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Red);
                    Circle(output, (int)avg.X, (int)avg.Y, (int)_circleRadiusMedianFilter.Filtered.X, System.Drawing.Color.Red);
                    CircleLocated(offset, _circleRadiusMedianFilter.Filtered.X, _circleRadiusMedianFilter.StandardDeviation);
                }
            }
            else
            {
                CircleCenter = null;
            }
        }
        #endregion

        #region Find Corners
        private void FindCorners(IImage blurredGray, IInputOutputArray output, Size size)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            using (var cornerDest = new Image<Gray, float>(blurredGray.Size))
            using (var matNormalized = new Image<Gray, float>(blurredGray.Size))
            using (var matScaled = new Image<Gray, float>(blurredGray.Size))
            {
                cornerDest.SetZero();

                int max = -1;
                int x = -1, y = -1;

                CvInvoke.CornerHarris(blurredGray, cornerDest, Profile.HarrisCornerBlockSize, Profile.HarrisCornerAperture, Profile.HarrisCornerK, BorderType.Default);

                CvInvoke.Normalize(cornerDest, matNormalized, 0, 255, NormType.MinMax, DepthType.Cv32F);
                CvInvoke.ConvertScaleAbs(matNormalized, matScaled, 10, 5);
                var minX = (size.Width / 2) - Profile.TargetImageRadius;
                var maxX = (size.Width / 2) + Profile.TargetImageRadius;
                var minY = (size.Height / 2) - Profile.TargetImageRadius;
                var maxY = (size.Height / 2) + Profile.TargetImageRadius;

                /* Go through all the returned points and find the one with the highest intensity.  This will be our corner */
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

                if (x > 0 && y > 0)
                {
                    _cornerMedianFilter.Add(new Point2D<float>(x, y));

                }

                var avg = _cornerMedianFilter.Filtered;
                if (avg != null)
                {
                    Circle(output, (int)avg.X, (int)avg.Y, 5, System.Drawing.Color.Blue);
                    Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Blue);
                    Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Blue);

                    var offset = new Point2D<double>(center.X - avg.X, center.Y - avg.Y);
                    CornerLocated(offset, _cornerMedianFilter.StandardDeviation);
                }
            }
        }
        #endregion

        FloatMedianFilter _rectP1 = new FloatMedianFilter();
        FloatMedianFilter _rectP2 = new FloatMedianFilter();
        FloatMedianFilter _rectP3 = new FloatMedianFilter();
        FloatMedianFilter _rectP4 = new FloatMedianFilter();

        #region Find Rotated Rectangles
        private void FindRectangles(IImage input, IInputOutputArray output, Size size)
        {
            UMat edges = new UMat();


            if (ShowLines)
            {
                var lines = CvInvoke.HoughLinesP(edges, Profile.HoughLinesRHO, Profile.HoughLinesTheta * (Math.PI / 180), Profile.HoughLinesThreshold, Profile.HoughLinesMinLineLength, Profile.HoughLinesMaxLineGap);
                foreach (var line in lines)
                {
                    Line(output, line.P1.X, line.P1.Y, line.P2.X, line.P2.Y, System.Drawing.Color.Yellow);
                }
            }

            if (Profile.UseCannyEdgeDetection)
            {
                CvInvoke.Canny(input, edges, Profile.CannyLowThreshold, Profile.CannyHighThreshold, Profile.CannyApetureSize, Profile.CannyGradient);
            }
            else
            {
                CvInvoke.Threshold(input, edges, Profile.ThresholdEdgeDetection, 255, ThresholdType.Binary);
            }

            using (var contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(edges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (var contour = contours[i])
                    using (var approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * Profile.PolygonEpsilonFactor, Profile.ContourFindOnlyClosed);
                        if (CvInvoke.ContourArea(approxContour, false) > Profile.ContourMinArea) //only consider contours with area greater than 250
                        {
                            var pts = approxContour.ToArray();

                            if (approxContour.Size == 4) //The contour has 4 vertices.
                            {
                                bool isRectangle = true;
                                var rectEdges = PointCollection.PolyLine(pts, true);

                                if (!Profile.FindIrregularPolygons)
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

                                    if (rect.Center.X > ((size.Width / 2) - Profile.TargetImageRadius) && rect.Center.X < ((size.Width / 2) + Profile.TargetImageRadius) &&
                                        rect.Center.Y > ((size.Height / 2) - Profile.TargetImageRadius) && rect.Center.Y < ((size.Height / 2) + Profile.TargetImageRadius))
                                    {
                                        if (rect.Size.Width > rect.Size.Height && Profile.FindLandScape ||
                                            rect.Size.Height > rect.Size.Width && Profile.FindPortrait)
                                        {
                                            var point1 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));
                                            var point2 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                            var point3 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                            var point4 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));

                                            var p1 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));
                                            var p2 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                            var p3 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                            var p4 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));


                                            _rectP1.Add(p1);
                                            _rectP2.Add(p2);
                                            _rectP3.Add(p3);
                                            _rectP4.Add(p4);


                                            /*
                                            CvInvoke.Line(output, point1, point2, new Bgr(System.Drawing.Color.Red).MCvScalar);
                                            CvInvoke.Line(output, point2, point3, new Bgr(System.Drawing.Color.Red).MCvScalar);
                                            CvInvoke.Line(output, point3, point4, new Bgr(System.Drawing.Color.Red).MCvScalar);
                                            CvInvoke.Line(output, point4, point1, new Bgr(System.Drawing.Color.Red).MCvScalar);
                                            */
                                        }
                                    }
                                }
                                
                            }
                            else if (Profile.FindIrregularPolygons)
                            {
                                var rectEdges = PointCollection.PolyLine(pts, true);
                                for (var idx = 0; idx < rectEdges.Length - 1; ++idx)
                                {
                                    CvInvoke.Line(output, rectEdges[idx].P1, rectEdges[idx].P2, new Bgr(System.Drawing.Color.LightBlue).MCvScalar);
                                }

                            }
                        }
                    }
                }

                var avg1 = _rectP1.Filtered;
                var avg2 = _rectP2.Filtered;
                var avg3 = _rectP3.Filtered;
                var avg4 = _rectP4.Filtered;

                if (avg1 != null && avg2 != null && avg3 != null && avg4 != null)
                {
                    Line(output, (int)avg1.X, (int)avg1.Y, (int)avg2.X, (int)avg2.Y, System.Drawing.Color.Red);
                    Line(output, (int)avg2.X, (int)avg2.Y, (int)avg3.X, (int)avg3.Y, System.Drawing.Color.Red);
                    Line(output, (int)avg3.X, (int)avg3.Y, (int)avg4.X, (int)avg4.Y, System.Drawing.Color.Red);
                    Line(output, (int)avg4.X, (int)avg4.Y, (int)avg1.X, (int)avg1.Y, System.Drawing.Color.Red);
                }
            }
        }
        #endregion

        public IImage PerformShapeDetection(Mat img)
        {
            if (img == null)
            {
                return null;
            }

            try
            {
                using (var gray = new Image<Gray, byte>(img.Bitmap))
                using (var blurredGray = new Image<Gray, float>(gray.Size))
                {
                    var output = ShowOriginalImage ? img : (IInputOutputArray)gray;

                    var input = (IImage)gray;
                    if (UseBlurredImage)
                    {
                        CvInvoke.GaussianBlur(gray, blurredGray, new System.Drawing.Size(5, 5), Profile.GaussianSigmaX);
                        input = blurredGray;
                    }

                    if (!Machine.Busy)
                    {
                        if (ShowCrossHairs) DrawCrossHairs(output, img.Size);
                        if (ShowCircles) FindCircles(input, output, img.Size);
                        if (ShowHarrisCorners) FindCorners(input, output, img.Size);
                        if (ShowRectangles) FindRectangles(input, output, img.Size);
                    }
                    else
                    {
                        _stabilizedPointCount = 0;
                    }

                    if (ShowOriginalImage) return img;
                    else if (UseBlurredImage) return blurredGray.Clone();

                    return gray.Clone();
                }
            }
            catch(Exception)
            {
                /*NOP, sometimes OpenCV acts a little funny. */
                return null;
            }
        }
    }
}