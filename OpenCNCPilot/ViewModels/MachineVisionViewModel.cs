using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Util;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public partial class MachineVisionViewModel : GCodeAppViewModelBase
    {
        FloatMedianFilter _cornerMedianFilter = new FloatMedianFilter(12, 3);
        FloatMedianFilter _circleMedianFilter = new FloatMedianFilter(12, 3);


        public MachineVisionViewModel(IMachine machine) : base(machine)
        {
            Profile = new Models.VisionProfile();
            SaveProfileCommand = new RelayCommand(SaveProfile);
            CaptureCameraCommand = new RelayCommand(CaptureCameraLocation);
            CaptureDrillLocationCommand = new RelayCommand(CaptureDrillLocation);
        }

        public override async Task InitAsync()
        {
            var profile = await Storage.GetAsync<Models.VisionProfile>("Vision.json");
            if (profile != null)
            {
                Profile = profile;
            }

        }

        public async void SaveProfile()
        {
            await Storage.StoreAsync(Profile, "Vision.json");
        }

        public RelayCommand SaveProfileCommand { get; private set; }

        private void Circle(IInputOutputArray img, int x, int y, int radius, System.Drawing.Color color, int thickness = 1)
        {
            if (!ShowOriginalImage)
            {
                color = System.Drawing.Color.White;
            }

            CvInvoke.Circle(img,
            new System.Drawing.Point(x, y), radius,
            new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);

        }

        private void Line(IInputOutputArray img, int x1, int y1, int x2, int y2, System.Drawing.Color color, int thickness = 1)
        {
            if(!ShowOriginalImage)
            {
                color = System.Drawing.Color.White;
            }

            CvInvoke.Line(img, new System.Drawing.Point(x1, y1),
                new System.Drawing.Point(x2, y2),
                new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
        }

        private Point2D<double> _circleCenter;
        public Point2D<double> CircleCenter
        {
            get { return _circleCenter; }
            set { Set(ref _circleCenter, value); }
        }


        private Point2D<double> _foundCorner;
        public Point2D<double> FoundCorner
        {
            get { return _foundCorner; }
            set { Set(ref _foundCorner, value); }
        }

        public Mat PerformShapeDetection(Mat img)
        {
            if(img == null)
            {
                return null;
            }

            using (var gray = new Image<Gray, byte>(img.Bitmap))
            using (var blurredGray = new Image<Gray, float>(gray.Size))
            using (var finalOutput = new Mat())
            {
                
                var whiteColor = new Bgr(System.Drawing.Color.White).MCvScalar;

                CvInvoke.GaussianBlur(gray, blurredGray, System.Drawing.Size.Empty, Profile.GaussianSigmaX);

                UMat pyrDown = new UMat();
                CvInvoke.PyrDown(blurredGray, pyrDown);
                CvInvoke.PyrUp(pyrDown, blurredGray);

                var destImage = ShowOriginalImage ? img : (IInputOutputArray)blurredGray;

                UMat edges = new UMat();

                if (Profile.UseCannyEdgeDetection)
                {
                    CvInvoke.Canny(blurredGray, edges, Profile.CannyLowThreshold, Profile.CannyHighThreshold, Profile.CannyApetureSize, Profile.CannyGradient);
                }
                else
                {
                    CvInvoke.Threshold(blurredGray, edges, Profile.ThresholdEdgeDetection, 255, ThresholdType.Binary);
                }

                if (ShowCircles)
                {
                    var circles = CvInvoke.HoughCircles(blurredGray, HoughType.Gradient, Profile.HoughCirclesDP, Profile.HoughCirclesMinDistance, Profile.HoughCirclesParam1, Profile.HoughCirclesParam2, Profile.HoughCirclesMinRadius, Profile.HoughCirclesMaxRadius);

                    var foundCircle = false;
                    foreach (var circle in circles)
                    {
                        if (circle.Center.X > ((img.Size.Width / 2) - Profile.TargetImageRadius) && circle.Center.X < ((img.Size.Width / 2) + Profile.TargetImageRadius) &&
                           circle.Center.Y > ((img.Size.Height / 2) - Profile.TargetImageRadius) && circle.Center.Y < ((img.Size.Height / 2) + Profile.TargetImageRadius))
                        {
                            _circleMedianFilter.Add(circle.Center.X, circle.Center.Y);

                            foundCircle = true;

                            break;
                        }
                    }

                    if (!foundCircle)
                    {
                        _circleMedianFilter.Add(null);
                    }

                    var avg = _circleMedianFilter.Filtered;
                    if (avg != null)
                    {
                        Line(destImage, 0, (int)avg.Y, img.Width, (int)avg.Y, System.Drawing.Color.Red);
                        Line(destImage, (int)avg.X, 0, (int)avg.X, img.Size.Height, System.Drawing.Color.Red);

                        if((avg.X >= ((img.Width / 2) - 1)) && avg.Y <= ((img.Width / 2) + 1) &&
                           (avg.Y >= ((img.Height / 2) - 1)) && avg.Y <= ((img.Height / 2) + 1))
                        {
                            Circle(destImage, (int)avg.X, (int)avg.Y, 3, System.Drawing.Color.Red);
                        }
                    }
                }

                if (ShowCrossHairs)
                {
                    var width = img.Size.Width;
                    var height = img.Size.Height;

                    var centerX = img.Size.Width / 2;
                    var centerY = img.Size.Height / 2;

                    Circle(destImage, centerX, centerY, Profile.TargetImageRadius, System.Drawing.Color.Yellow);

                    Line(destImage, 0, centerY, centerX - Profile.TargetImageRadius, centerY, System.Drawing.Color.Yellow);
                    Line(destImage, centerX + Profile.TargetImageRadius, centerY, width, centerY, System.Drawing.Color.Yellow);

                    Line(destImage, centerX, 0, centerX, centerY - Profile.TargetImageRadius,  System.Drawing.Color.Yellow );
                    Line(destImage, centerX, centerY + Profile.TargetImageRadius, centerX, height, System.Drawing.Color.Yellow);
                }


                if (ShowLines)
                {
                    var lines = CvInvoke.HoughLinesP(edges, Profile.HoughLinesRHO, Profile.HoughLinesTheta * (Math.PI / 180), Profile.HoughLinesThreshold, Profile.HoughLinesMinLineLength, Profile.HoughLinesMaxLineGap);
                    foreach (var line in lines)
                    {
                        Line(destImage, line.P1.X, line.P1.Y, line.P2.X, line.P2.Y, System.Drawing.Color.Beige);
                    }
                }

                if (ShowHarrisCorners)
                {

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
                        var minX = (img.Size.Width / 2) - Profile.TargetImageRadius;
                        var maxX = (img.Size.Width / 2) + Profile.TargetImageRadius;
                        var minY = (img.Size.Height / 2) - Profile.TargetImageRadius;
                        var maxY = (img.Size.Height / 2) + Profile.TargetImageRadius;

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

                        var filtered = _cornerMedianFilter.Filtered;
                        if (filtered != null)
                        {
                            Circle(destImage, (int)filtered.X, (int)filtered.Y, 5, System.Drawing.Color.Blue);
                            Line(destImage, 0, (int)filtered.Y, img.Width, (int)filtered.Y, System.Drawing.Color.Blue);
                            Line(destImage, (int)filtered.X, 0, (int)filtered.X, img.Height, System.Drawing.Color.Blue);
                        }
                    }
                }


                #region Calculate Polygons/Contours
                var triangleList = new List<Triangle2DF>();
                var boxList = new List<RotatedRect>(); //a box is a rotated rectangle

                if (ShowRectangles)
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
                #endregion

                return (destImage as Mat).Clone();
            }
        }

        private Point2D<double> _drillWorkLocation;

        public void CaptureDrillLocation()
        {
            _drillWorkLocation = new Point2D<double>(Machine.NormalizedPosition.X, Machine.NormalizedPosition.Y);
        }

        public async void CaptureCameraLocation()
        {
            var deltaX = Machine.NormalizedPosition.X - _drillWorkLocation.X;
            var deltaY = Machine.NormalizedPosition.Y - _drillWorkLocation.Y;
            Machine.Settings.PositioningCamera.Tool1Offset = new Point2D<double>(deltaX, deltaY);
            await Machine.MachineRepo.SaveAsync();
        }

        public RelayCommand CaptureDrillLocationCommand { get; private set; }

        public RelayCommand CaptureCameraCommand { get; private set; }
    }
}
