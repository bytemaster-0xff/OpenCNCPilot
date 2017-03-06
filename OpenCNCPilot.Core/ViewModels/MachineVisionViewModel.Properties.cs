using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineVisionViewModel
    {

        public string CannyLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/feature_detection.html"; } }
        public string CannyLink2 { get { return "https://en.wikipedia.org/wiki/Canny_edge_detector"; } }

        public string CannyLowThresholdHelp { get { return "Threshold for Line Detection"; } }
        public double CannyLowThreshold { get; set; }

        public string CannyHighThresholdHelp { get { return "Recommended to ve set to three times the lower threshold"; } }
        public double CannyHighThreshold { get; set; }

        public string CannyHighThresholdTracksLowThresholdHelp { get { return "Force High Threshold to Map to 3x Low Threshold"; } }
        public bool CannyHighThresholdTracksLowThreshold { get; set; }

        public string CannyApetureSizeHelp { get { return "The size of the Sobel kernel to be used internally"; } }
        public double CannyApetureSize { get; set; }

        public string CannyGradientHelp { get { return "a flag, indicating whether a more accurate  norm  should be used to calculate the image gradient magnitude ( L2gradient=true ), or whether the default  norm  is enough ( L2gradient=false )."; } }
        public bool CannyGradient { get; set; }


        public string HoughLinesLink { get { return "http://docs.opencv.org/2.4/doc/tutorials/imgproc/imgtrans/hough_lines/hough_lines.html"; } }

        public string HoughLinesRHOHelp { get { return "The resolution of the parameter R in pixels."; } }
        public double HoughLinesRHO {get; set;}
        public string HoughLinesThetaHelp { get { return "The resolution of the parameter Theta in Degrees."; } }
        public double HoughLinesTheta { get; set; }

        public string HoughLinesThresholdHelp { get { return "The minimum number of intersections to detect a line."; } }
        public double HoughLinesThreshold { get; set; }

        public string HoughLinesMinLineHelp { get { return "The minimum number of points that can form a line. Lines with less than this number of points are disregarded."; } }
        public double HoughLinesMinLineLength { get; set; }

        public string HoughLinesMaxLineGapHelp { get { return "The maximum gap between two points to be considered in the same line."; } }
        public double HoughLinesMaxLineGap { get; set; }
  
    }
}
