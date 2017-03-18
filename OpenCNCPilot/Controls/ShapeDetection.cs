using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using LagoVista.GCode.Sender.Models;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.Controls
{
    public partial class ImageSensorControl
    {
        public void CornerDetection(VisionProfile profile, Mat img)
        {
            var output = new Mat();
            var outputNormalized = new Mat();
            var outputNormalizedScaled = new Mat();
            CvInvoke.CornerHarris(img, output, profile.HarrisCornerBlockSize, profile.HarrisCornerAperture, profile.HarrisCornerK, BorderType.Default);
            CvInvoke.Normalize(output, outputNormalized, 0, 255, NormType.MinMax, DepthType.Cv32F, null);
            CvInvoke.ConvertScaleAbs(outputNormalized, outputNormalizedScaled,5,5);

            for (int j = 0; j < outputNormalized.Rows; j++)
            {
                for (int i = 0; i < outputNormalized.Cols; i++)
                {
                  //  if ((int)outputNormalized.GetData(j,i) > profile.HarrisCornerThreshold)
                    {
                     //   circle(outputNormalizedScaled, Point(i, j), 5, Scalar(0), 2, 8, 0);
                    }
                }
            }

        }

        public Result PerformShapeDetection(VisionProfile profile, Mat img)
        {
            using (var gray = new Image<Gray, byte>(img.Bitmap))
            using (var blurredGray = new Mat())
            using (var finalOutput = new Mat())
            {

                //K Must always be odd.

                CvInvoke.GaussianBlur(gray, blurredGray, System.Drawing.Size.Empty, profile.GaussianSigmaX);

                //Convert the image to grayscale and filter out the noise
                //UMat uimage = new UMat();
                //CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

                //use image pyr to remove noise
                UMat pyrDown = new UMat();
                CvInvoke.PyrDown(img, pyrDown);
                CvInvoke.PyrUp(pyrDown, img);

                //Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

                var circles = CvInvoke.HoughCircles(img, HoughType.Gradient,profile.HoughCirclesDP, profile.HoughCirclesMinDistance, profile.HoughCirclesParam1, profile.HoughCirclesParam2, profile.HoughCirclesMinRadius, profile.HoughCirclesMaxRadius);

                UMat cannyEdges = new UMat();
                CvInvoke.Canny(img, cannyEdges, profile.CannyLowThreshold, profile.CannyHighThreshold, profile.CannyApetureSize, profile.CannyGradient);

                var lines = CvInvoke.HoughLinesP(cannyEdges,profile.HoughLinesRHO, profile.HoughLinesTheta, profile.HoughLinesThreshold, profile.HoughLinesMinLineLength, profile.HoughLinesMaxLineGap);
                               
                List<Triangle2DF> triangleList = new List<Triangle2DF>();
                List<RotatedRect> boxList = new List<RotatedRect>(); //a box is a rotated rectangle

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
                }


                var results = new Result();
                results.Triangles = triangleList;
                results.Rects = boxList;
                results.Circles = circles.ToList();
                results.Lines = lines.ToList();

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
    }
}
