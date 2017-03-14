using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Models
{
    public class VisionProfile
    {
        public void RaisePropertyChanged()
        {

        }

        public string Name { get; set; }

        private double _polygonEpsilon = 0;
        public double PolygonEpsilon
        {
            get { return _polygonEpsilon; }
            set { _polygonEpsilon = value; RaisePropertyChanged(); }
        }

        private int _harrisCornerApeture = 0;
        public int HarrisCornerAperture
        {
            get { return _harrisCornerApeture; }
            set { _harrisCornerApeture = value; RaisePropertyChanged(); }
        }
        private int _harrisCornerBlockSize = 0;
        public double HarrisCornerBlockSize { get; set; }
        private int _harrisCornerK = 0;
        public int HarrisCornerK { get; set; }

        private int _guassianKSize = 5;
        public int GaussianKSize { get; set; } = 5;
        private double _guassianSigmaX = 2;
        public double GaussianSigmaX { get; set; } = 2;
        private double _guasianSigmaY = 2;
        public double GaussianSigmaY { get; set; } = 2;

        private double _cannyLowThreshold = 5;
        public double CannyLowThreshold { get; set; } = 5;
        private double _cannyHighThreshold = 3 * 5;
        public double CannyHighThreshold { get { return CannyLowThreshold * 3; } set { } }
        private int _cannyApetureSize = 3;
        public int CannyApetureSize { get; set; } = 3;
        private bool CannyGrandian = true;
        public bool CannyGradient { get; set; }


        private double _houghLinesRHO = 1;
        public double HoughLinesRHO { get; set; } = 1;
        private double _houghLinesTheta = 0;
        public double HoughLinesTheta { get; set; }
        private int _houghLinesThreshold = 0;
        public int HoughLinesThreshold { get; set; }
        private double _houghLinesMinLength = 5;
        public double HoughLinesMinLineLength { get; set; }
        private double _houghLinesMaxLineGap = 5;
        public double HoughLinesMaxLineGap { get; set; }


        private double _houghCirclesDP = 2;
        public double HoughCirclesDP
        {
            get { return _houghCirclesDP; }
            set { _houghCirclesDP = value; RaisePropertyChanged(); }
        }
        private double _houghLinesCircleMinDistance = 32;
        public double HoughCirclesMinDistance
        {
            get { return _houghLinesCircleMinDistance; }
            set { _houghLinesCircleMinDistance = value; RaisePropertyChanged(); }
        }
        private double _houghCirclesParam1 = 100;
        public double HoughCirclesParam1
        {
            get { return _houghCirclesParam1; }
            set { _houghCirclesParam1 = value; RaisePropertyChanged(); }
        }
        private double _houghCirclesParam2 = 100;
        public double HoughCirclesParam2
        {
            get { return _houghCirclesParam2; }
            set { _houghCirclesParam2 = value; RaisePropertyChanged(); }
        }
        private int _houghCirclesMinRadius = 50;
        public int HoughCirclesMinRadius
        {
            get { return _houghCirclesMinRadius; }
            set { _houghCirclesMinRadius = value; RaisePropertyChanged(); }
        }
        private int _houghCirclesMaxRadius = 150;
        public int HoughCirclesMaxRadius
        {
            get { return _houghCirclesMaxRadius; }
            set { _houghCirclesMaxRadius = value; RaisePropertyChanged(); }
        }
    }
}
