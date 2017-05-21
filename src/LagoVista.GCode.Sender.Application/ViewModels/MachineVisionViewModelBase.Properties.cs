﻿using LagoVista.GCode.Sender.ViewModels;
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
        public bool ShowPolygons { get; set; } = false;
        public bool ShowRectangles { get; set; } = false;
        public bool ShowCircles { get; set; } = true;
        public bool ShowLines { get; set; } = false;
        public bool ShowCrossHairs { get; set; } = true;
        public bool ShowHarrisCorners { get; set; } = false;
        public bool ShowOriginalImage { get; set; } = true;

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

        public MachineControlViewModel MachineControls { get; private set; }

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
                if (value && !_hasFrame)
                {
                    CaptureStarted();
                }

                if (!value && _hasFrame)
                {
                    CaptureEnded();
                }

                Set(ref _hasFrame, value);
            }
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
            }
        }

        bool _pictureInPicture = false;
        public bool PictureInPicture
        {
            get { return _pictureInPicture; }
            set { Set(ref _pictureInPicture, value); }
        }
    }
}
