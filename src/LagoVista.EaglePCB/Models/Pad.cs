﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Pad
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Drill { get; set; }
        public string Shape { get; set; }
        public string RotateStr { get; set; }

        public static Pad Create(XElement element)
        {
            return new Pad()
            {
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                Drill = element.GetDouble("drill"),
                Name = element.GetString("name"),
                Shape = element.GetString("shape"),
                RotateStr = element.GetString("rot")
            };
        }
    }
}