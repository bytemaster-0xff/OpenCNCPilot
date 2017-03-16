﻿using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Models
{
    public class VisionProfile : ModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value;  RaisePropertyChanged(); }
        }

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
        private double _harrisCornerBlockSize = 0;
        public double HarrisCornerBlockSize
        {
            get { return _harrisCornerBlockSize; }
            set { _harrisCornerBlockSize = value; RaisePropertyChanged(); }
        }

        private int _harrisCornerK = 0;
        public int HarrisCornerK
        {
            get { return _harrisCornerK; }
            set { _harrisCornerK = value; RaisePropertyChanged(); }
        }

        private int _guassianKSize = 5;
        public int GaussianKSize
        {
            get { return _guassianKSize; }
            set { _guassianKSize = value; RaisePropertyChanged(); }
        }
        private double _guassianSigmaX = 2;
        public double GaussianSigmaX
        {
            get { return _guassianSigmaX; }
            set { _guassianSigmaX = value; RaisePropertyChanged(); }
        }
        private double _guasianSigmaY = 2;
        public double GaussianSigmaY
        {
            get { return _guasianSigmaY; }
            set { _guasianSigmaY = value; RaisePropertyChanged(); }
        }

        private double _cannyLowThreshold = 5;
        public double CannyLowThreshold
        {
            get { return _cannyLowThreshold; }
            set
            {
                _cannyLowThreshold = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CannyHighThreshold));
            }
        }

        public double CannyHighThreshold { get { return CannyLowThreshold * 3; } set { } }
        private int _cannyApetureSize = 3;
        public int CannyApetureSize
        {
            get { return _cannyApetureSize; }
            set { _cannyApetureSize = value; RaisePropertyChanged(); }
        }

        private bool _cannyGradiant = true;
        public bool CannyGradient
        {
            get { return _cannyGradiant; }
            set { _cannyGradiant = value; RaisePropertyChanged(); }
        }


        private double _houghLinesRHO = 1;
        public double HoughLinesRHO
        {
            get { return _houghLinesRHO; }
            set { _houghLinesRHO = value; RaisePropertyChanged(); }
        }
        private double _houghLinesTheta = 0;
        public double HoughLinesTheta
        {
            get { return _houghLinesTheta; }
            set { _houghLinesTheta = value; RaisePropertyChanged(); }
        }
        private int _houghLinesThreshold = 0;
        public int HoughLinesThreshold
        {
            get { return _houghLinesThreshold; }
            set { _houghLinesThreshold = value; RaisePropertyChanged(); }
        }
        private double _houghLinesMinLength = 5;
        public double HoughLinesMinLineLength
        {
            get { return _houghLinesMinLength; }
            set { _houghLinesMinLength = value; RaisePropertyChanged(); }
        }
        private double _houghLinesMaxLineGap = 5;
        public double HoughLinesMaxLineGap
        {
            get { return _houghLinesMaxLineGap; }
            set { _houghLinesMaxLineGap = value; RaisePropertyChanged(); }
        }

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