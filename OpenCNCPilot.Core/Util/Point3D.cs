﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Util
{
    public class Point3D
    {
        public Point3D(double x,double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X {get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

    }
}
