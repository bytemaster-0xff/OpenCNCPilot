using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineVisionViewModel
    {
        public bool ShowRectangles { get; set; }
        public bool ShowCircles { get; set; }
        public bool ShowLines { get; set; }
        public bool ShowHarrisCorners { get; set; }
      


        public string PolygonHelp { get { return "http://docs.opencv.org/2.4/doc/tutorials/imgproc/shapedescriptors/bounding_rects_circles/bounding_rects_circles.html?highlight=approxpolydp"; } }

        public string PolygonEpsilonHelp { get { return "Parameter specifying the approximation accuracy. This is the maximum distance between the original curve and its approximation"; } }

        public double PolygonEpsilon { get; set; }

        public string HarrisCornerLink { get { return "http://docs.opencv.org/2.4/doc/tutorials/features2d/trackingmotion/harris_detector/harris_detector.html"; } }
        public string HarrisCornerApertureHelp { get { return "Apertur parameter for Sobel operation"; } }
        public int HarrisCornerAperture { get; set; }
        public string HarrisCornerBlockSizeString { get { return "Neighborhood Size"; } }
        public double HarrisCornerBlockSize { get; set; }
        public string HarrisCornerKHelp { get { return "Harris detector free parameter."; } }
        public int HarrisCornerK { get; set; }


        public string GaussianBlurLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/filtering.html?highlight=gaussianblur#cv2.GaussianBlur"; } }
        public string GuassianKSizeHelp { get { return "Gaussian kernel size. ksize.width and ksize.height can differ but they both must be positive and odd. Or, they can be zero’s and then they are computed from sigma* "; } }
        public int GaussianKSize { get; set; } = 5;
        public string GaussianSigmaXHelp { get { return "Gaussian kernel standard deviation in X direction."; } }
        public double GaussianSigmaX { get; set; } = 2;
        public string GaussianSigmaYHelp { get { return "Gaussian kernel standard deviation in Y direction; if sigmaY is zero, it is set to be equal to sigmaX, if both sigmas are zeros, they are computed from ksize.width and ksize.height , respectively (see getGaussianKernel() for details); to fully control the result regardless of possible future modifications of all this semantics, it is recommended to specify all of ksize, sigmaX, and sigmaY"; } }
        public double GaussianSigmaY { get; set; } = 2;
        

        public string CannyLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/feature_detection.html"; } }
        public string CannyLink2 { get { return "https://en.wikipedia.org/wiki/Canny_edge_detector"; } }
        public string CannyLowThresholdHelp { get { return "Threshold for Line Detection"; } }
        public double CannyLowThreshold { get; set; } = 5;
        public string CannyHighThresholdHelp { get { return "Recommended to ve set to three times the lower threshold"; } }
        public double CannyHighThreshold { get { return CannyLowThreshold * 3; } set { } }
        public string CannyHighThresholdTracksLowThresholdHelp { get { return "Force High Threshold to Map to 3x Low Threshold"; } }
        public bool CannyHighThresholdTracksLowThreshold { get; set; }
        public string CannyApetureSizeHelp { get { return "The size of the Sobel kernel to be used internally"; } }
        public int CannyApetureSize { get; set; } = 3;
        public string CannyGradientHelp { get { return "a flag, indicating whether a more accurate  norm  should be used to calculate the image gradient magnitude ( L2gradient=true ), or whether the default  norm  is enough ( L2gradient=false )."; } }
        public bool CannyGradient { get; set; }


        public string HoughLinesLink { get { return "http://docs.opencv.org/2.4/doc/tutorials/imgproc/imgtrans/hough_lines/hough_lines.html"; } }
        public string HoughLinesRHOHelp { get { return "The resolution of the parameter R in pixels."; } }
        public double HoughLinesRHO { get; set; } = 1;
        public string HoughLinesThetaHelp { get { return "The resolution of the parameter Theta in Degrees."; } }
        public double HoughLinesTheta { get; set; }
        public string HoughLinesThresholdHelp { get { return "The minimum number of intersections to detect a line."; } }
        public int HoughLinesThreshold { get; set; }
        public string HoughLinesMinLineHelp { get { return "The minimum number of points that can form a line. Lines with less than this number of points are disregarded."; } }
        public double HoughLinesMinLineLength { get; set; }
        public string HoughLinesMaxLineGapHelp { get { return "The maximum gap between two points to be considered in the same line."; } }
        public double HoughLinesMaxLineGap { get; set; }

        public string HoughCirclesLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/feature_detection.html#houghcircles"; } }
        public string HoughCirclesDPHelp { get { return "Inverse ratio of the accumulator resolution to the image resolution. For example, if dp=1 , the accumulator has the same resolution as the input image. If dp=2 , the accumulator has half as big width and height"; } }
        public double HoughCirclesDP { get; set; } = 2;
        public string HoughCirclesMinDistanceHelp { get { return "Minimum distance between the centers of the detected circles. If the parameter is too small, multiple neighbor circles may be falsely detected in addition to a true one. If it is too large, some circles may be missed."; } }
        public double HoughCirclesMinDistance { get; set; } = 32;
        public string HoughCirclesParam1Help { get { return "Higher threshold of the two passed to the Canny() edge detector (the lower one is twice smaller)."; } }
        public double HoughCirclesParam1 { get; set; } = 100;
        public string HoughCirclesParam2Help { get { return " it is the accumulator threshold for the circle centers at the detection stage. The smaller it is, the more false circles may be detected. Circles, corresponding to the larger accumulator values, will be returned first."; } }
        public double HoughCirclesParam2 { get; set; } = 100;
        public string HoughCirclesMinRadiusHelp { get { return "Minimum Radius"; } }
        public int HoughCirclesMinRadius { get; set; } = 50;
        public string HoughCirclesMaxRadiusHelp { get { return "Maximum Radius"; } }
        public int HoughCirclesMaxRadius { get; set; } = 150;

    }
}
