using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Component
    {
        public string Name { get; set; }
        public string LibraryName { get; set; }
        public string PackageName { get; set; }
        public string Value { get; set; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public string Rotate { get; set; }

        public int Layer
        {
            get
            {
                return (Rotate != null && Rotate.StartsWith("M")) ? 16 : 1;
            }
        }

        public Package Package { get; set; }

        public List<Pad> Pads
        {
            get
            {
                var pads = new List<Pad>();

                var rad = 0.0;
                if (!String.IsNullOrEmpty(Rotate))
                {
                    var degrees = 0.0;
                    if (Rotate.Substring(0, 1) == "R")
                    {
                        degrees = Convert.ToDouble(Rotate.Substring(1));

                    }
                    else if (Rotate.Substring(0, 1) == "L")
                    {
                        degrees = Convert.ToDouble(Rotate.Substring(1));
                    }

                    rad = degrees * Math.PI / 180;
                }

                foreach (var pad in Package.Pads)
                {
                    pads.Add(new Pad()
                    {
                        OriginX = pad.OriginX,
                        OriginY = pad.OriginY,
                        X = X.Value + ((pad.X * Math.Cos(rad)) - (pad.Y * Math.Sin(rad))),
                        Y = Y.Value + ((pad.Y * Math.Sin(rad)) + (pad.X * Math.Cos(rad))),
                        Shape = pad.Shape,
                        DrillDiameter = pad.DrillDiameter,
                    });
                }

                return pads;
            }

        }

        public List<SMD> SMDPads
        {
            get
            {
                var smdPads = new List<SMD>();

                var rad = 0.0;
                if (!String.IsNullOrEmpty(Rotate))
                {
                    var degrees = 0.0;
                    if (Rotate.Substring(0, 1) == "R")
                    {
                        degrees = Convert.ToDouble(Rotate.Substring(1));

                    }
                    else if (Rotate.Substring(0, 1) == "L")
                    {
                        degrees = Convert.ToDouble(Rotate.Substring(1));
                    }

                    rad = degrees * Math.PI / 180;
                }

                foreach (var smd in Package.SMDs)
                {
                    var x1 = (smd.OriginX) - (smd.DX / 2);
                    var y1 = (smd.OriginY) - (smd.DY / 2);
                    var x2 = (smd.OriginX) + (smd.DX / 2);
                    var y2 = (smd.OriginY) + (smd.DY / 2);

                    if (rad != 0)
                    {
                        x1 = x1 * Math.Cos(rad) - y1 * Math.Sin(rad);
                        y1 = y1 * Math.Sin(rad) + y1 * Math.Cos(rad);
                        x2 = x2 * Math.Cos(rad) - y2 * Math.Sin(rad);
                        y2 = y2 * Math.Sin(rad) + y2 * Math.Cos(rad);
                    }

                    smdPads.Add(new SMD()
                    {
                        OriginX = smd.OriginX,
                        OriginY = smd.OriginY,
                        X1 = this.X.Value + x1,
                        Y1 = this.Y.Value + y1,
                        X2 = this.X.Value + x2,
                        Y2 = this.Y.Value + y2,
                        DX = smd.DX,
                        DY = smd.DY
                    });
                }


                return smdPads;
            }
        }

        public static Component Create(XElement element)
        {
            return new Component()
            {
                Name = element.GetString("name"),
                LibraryName = element.GetString("library"),
                PackageName = element.GetString("package"),
                Rotate = element.GetString("rot"),
                Value = element.GetString("value"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y")
            };
        }
    }
}
