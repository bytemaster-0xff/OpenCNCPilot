using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool Included { get; set; }
        public string Function { get; set; }
        public string Notes { get; set; }
      
        public int Layer
        {
            get
            {
                return (Rotate != null && Rotate.StartsWith("M")) ? 16 : 1;
            }
        }

        public double RotateAngle
        {
            get
            {
                return Rotate.ToAngle();
            }
        }

        public Package Package { get; set; }

        public List<Pad> Pads
        {
            get
            {
                var pads = new List<Pad>();
                foreach (var pad in Package.Pads)
                {
                    var rotatedPad = pad.ApplyRotation(Rotate.ToAngle());
                    rotatedPad.X += X.Value;
                    rotatedPad.Y += Y.Value;                    
                    pads.Add(rotatedPad);
                }
           
                return pads;
            }
        }

        public List<Hole> Holes
        {
            get
            {
                var holes = new List<Hole>();
                foreach(var hole in Package.Holes)
                {
                    var rotatedHole = hole.ApplyRotation(Rotate.ToAngle());
                    rotatedHole.X += X.Value;
                    rotatedHole.Y += Y.Value;
                    holes.Add(rotatedHole);
                }


                return holes;
            }
        }

        public List<SMD> SMDPads
        {
            get
            {
                var smdPads = new List<SMD>();

                foreach (var smd in Package.SMDs)
                {
                    
                    var rotatedSMD = smd.ApplyRotation(Rotate.ToAngle());

                    if(!String.IsNullOrEmpty(Rotate))
                    {
                    }

                    rotatedSMD.X1 += X.Value;
                    rotatedSMD.Y1 += Y.Value;
                    rotatedSMD.X2 += X.Value;
                    rotatedSMD.Y2 += Y.Value;
                    smdPads.Add(smd);
                }

                return smdPads;
            }
        }

        public string Key => PackageName + "-" + Value;

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

        public Component Clone()
        {
            return new Component()
            {
                Name = Name,
                LibraryName = LibraryName,
                PackageName = PackageName,
                Rotate = Rotate,
                Value = Value,
                X = X,
                Y = Y,
                Package = Package.Clone(),
                Included = Included,
                Function = Function,
                Notes = Notes,
            };
        }
    }
}
