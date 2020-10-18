using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Models;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public abstract partial class MachineVisionViewModelBase
    {
        bool _showPolygons = false;
        bool _showRectangles = false;
        bool _showCircles = false;
        bool _showLines = false;
        bool _showCrossHairs = true;
        bool _showHarrisCorners = false;
        bool _showOriginalImage = true;
        bool _useBlurredImage = true;

        public bool ShowPolygons 
        {
            get { return _showPolygons; }
            set { Set(ref _showPolygons, value);  }
        }
        public bool ShowRectangles
        {
            get { return _showRectangles; }
            set { Set(ref _showRectangles, value); }
        }
        public bool ShowCircles
        {
            get { return _showCircles; }
            set { Set(ref _showCircles, value); }
        }
        public bool ShowLines
        {
            get { return _showLines; }
            set { Set(ref _showLines, value); }
        }
        public bool ShowCrossHairs
        {
            get { return _showCrossHairs; }
            set { Set(ref _showCrossHairs, value); }
        }
        public bool ShowHarrisCorners
        {
            get { return _showHarrisCorners; }
            set { Set(ref _showHarrisCorners, value); }
        }
        public bool ShowOriginalImage
        {
            get { return _showOriginalImage; }
            set { Set(ref _showOriginalImage, value); }
        }        
        
        public bool UseBlurredImage
        {
            get { return _useBlurredImage; }
            set { Set(ref _useBlurredImage, value); }
        }

        public string PolygonHelp { get { return "http://docs.opencv.org/2.4/doc/tutorials/imgproc/shapedescriptors/bounding_rects_circles/bounding_rects_circles.html?highlight=approxpolydp"; } }
        public string PolygonEpsilonHelp { get { return "Parameter specifying the approximation accuracy. This is the maximum distance between the original curve and its approximation"; } }

        public string HarrisCornerLink { get { return "http://docs.opencv.org/2.4/doc/tutorials/features2d/trackingmotion/harris_detector/harris_detector.html"; } }
        public string HarrisCornerApertureHelp { get { return "Apertur parameter for Sobel operation"; } }
        public string HarrisCornerBlockSizeString { get { return "Neighborhood Size"; } }
        public string HarrisCornerKHelp { get { return "Harris detector free parameter."; } }

        public string CannyLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/feature_detection.html"; } }
        public string CannyLink2 { get { return "https://en.wikipedia.org/wiki/Canny_edge_detector"; } }
        public string CannyLowThresholdHelp { get { return "Threshold for Line Detection"; } }
        public string CannyHighThresholdHelp { get { return "Recommended to ve set to three times the lower threshold"; } }
        public string CannyHighThresholdTracksLowThresholdHelp { get { return "Force High Threshold to Map to 3x Low Threshold"; } }
        public string CannyApetureSizeHelp { get { return "The size of the Sobel kernel to be used internally"; } }
        public string CannyGradientHelp { get { return "a flag, indicating whether a more accurate  norm  should be used to calculate the image gradient magnitude ( L2gradient=true ), or whether the default  norm  is enough ( L2gradient=false )."; } }

        public string HoughLinesLink { get { return "http://docs.opencv.org/2.4/doc/tutorials/imgproc/imgtrans/hough_lines/hough_lines.html"; } }
        public string HoughLinesRHOHelp { get { return "The resolution of the parameter R in pixels."; } }
        public string HoughLinesThetaHelp { get { return "The resolution of the parameter Theta in Degrees."; } }
        public string HoughLinesThresholdHelp { get { return "The minimum number of intersections to detect a line."; } }
        public string HoughLinesMinLineHelp { get { return "The minimum number of points that can form a line. Lines with less than this number of points are disregarded."; } }
        public string HoughLinesMaxLineGapHelp { get { return "The maximum gap between two points to be considered in the same line."; } }

        public string HoughCirclesLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/feature_detection.html#houghcircles"; } }
        public string HoughCirclesDPHelp { get { return "Inverse ratio of the accumulator resolution to the image resolution. For example, if dp=1 , the accumulator has the same resolution as the input image. If dp=2 , the accumulator has half as big width and height"; } }
        public string HoughCirclesMinDistanceHelp { get { return "Minimum distance between the centers of the detected circles. If the parameter is too small, multiple neighbor circles may be falsely detected in addition to a true one. If it is too large, some circles may be missed."; } }
        public string HoughCirclesParam1Help { get { return "Higher threshold of the two passed to the Canny() edge detector (the lower one is twice smaller)."; } }
        public string HoughCirclesParam2Help { get { return " it is the accumulator threshold for the circle centers at the detection stage. The smaller it is, the more false circles may be detected. Circles, corresponding to the larger accumulator values, will be returned first."; } }
        public string HoughCirclesMinRadiusHelp { get { return "Minimum Radius"; } }
        public string HoughCirclesMaxRadiusHelp { get { return "Maximum Radius"; } }

        public string GaussianBlurLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/filtering.html?highlight=gaussianblur#cv2.GaussianBlur"; } }
        public string GaussianKSizeHelp { get { return "Gaussian kernel size. ksize.width and ksize.height can differ but they both must be positive and odd. Or, they can be zero’s and then they are computed from sigma* "; } }
        public string GaussianSigmaXHelp { get { return "Gaussian kernel standard deviation in X direction."; } }
        public string GaussianSigmaYHelp { get { return "Gaussian kernel standard deviation in Y direction; if sigmaY is zero, it is set to be equal to sigmaX, if both sigmas are zeros, they are computed from ksize.width and ksize.height , respectively (see getGaussianKernel() for details); to fully control the result regardless of possible future modifications of all this semantics, it is recommended to specify all of ksize, sigmaX, and sigmaY"; } }

        private bool _loadingMask;
        public bool LoadingMask
        {
            get { return _loadingMask; }
            set { Set(ref _loadingMask, value); }
        }

        private BitmapSource _primaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
        public BitmapSource PrimaryCapturedImage
        {
            get { return _primaryCapturedImage; }
            set { Set(ref _primaryCapturedImage, value); }
        }

        private BitmapSource _secondaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
        public BitmapSource SecondaryCapturedImage
        {
            get { return _secondaryCapturedImage; }
            set { Set(ref _secondaryCapturedImage, value); }
        }

        public Sender.ViewModels.MachineControlViewModel MachineControls { get; private set; }

        private bool _areToolSettingsVisible;
        public bool AreToolSettingsVisible
        {
            get { return _areToolSettingsVisible; }
            set { Set(ref _areToolSettingsVisible, value); }
        }

        private bool _hasFrame = false;
        public bool HasFrame
        {
            get { return _hasFrame; }
            set
            {
                var oldHasFrame = _hasFrame;
                Set(ref _hasFrame, value);

                if (value && !oldHasFrame)
                {
                    CaptureStarted();
                }

                if (!value && oldHasFrame)
                {
                    CaptureEnded();
                }
            }
        }


        private VisionProfile _profile;
        public VisionProfile Profile
        {
            get { return _profile; }
            set { Set(ref _profile, value); }
        }


        protected virtual void CaptureStarted() { }

        protected virtual void CaptureEnded() { }

        bool _showTopCamera = true;
        public bool ShowTopCamera
        {
            get { return _showTopCamera; }
            set
            {
                if (value)
                {
                    Profile = _topCameraProfile;
                    ShowBottomCamera = false;
                }

                Set(ref _showTopCamera, value);
                UseTopCamera = true;
                UseBottomCamera = false;
            }
        }

        bool _showBottomCamera = false;
        public bool ShowBottomCamera
        {
            get { return _showBottomCamera; }
            set
            {
                if (value)
                {
                    Profile = _bottomCameraProfile;
                    ShowTopCamera = false;
                }

                Set(ref _showBottomCamera, value);
                UseBottomCamera = true;
                UseTopCamera = false;
            }
        }

        bool _pictureInPicture = true;
        public bool PictureInPicture
        {
            get { return _pictureInPicture; }
            set { Set(ref _pictureInPicture, value); }
        }


        private Point2D<double> _circleCenter;
        public Point2D<double> CircleCenter
        {
            get { return _circleCenter; }
            set
            {
                if (value == null)
                {
                    Set(ref _circleCenter, null);
                }
                else if (_circleCenter == null || (_circleCenter.X != value.X ||
                    _circleCenter.Y != value.Y))
                {
                    Set(ref _circleCenter, value);
                }
            }
        }

        private Point2D<double> _standardDeviation;
        public Point2D<double> StandardDeviation
        {
            get { return _standardDeviation; }
            set
            {
                if (value == null)
                {
                    Set(ref _standardDeviation, null);
                }
                else if (_standardDeviation == null || (_standardDeviation.X != value.X || _standardDeviation.Y != value.Y))
                {
                    Set(ref _standardDeviation, value);
                }
            }
        }

    }
}
