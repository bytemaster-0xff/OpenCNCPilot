using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Hole
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Drill { get; set; }

        public Package Package { get; set; }

        public static Hole Create(XElement element)
        {
            return new Models.Hole()
            {
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                Drill = element.GetDouble("drill"),
                Name = element.GetString("name")
            };
        }

        public Hole ApplyRotation(double angle)
        {
            var hole = this.MemberwiseClone() as Hole;
            if (angle == 0)
            {
                return hole;
            }

            var rotated = new Point2D<double>(hole.X, hole.Y).Rotate(angle);

            hole.X = Math.Round(rotated.X, 6);
            hole.Y = Math.Round(rotated.Y, 6);

            return hole;
        }


        public override string ToString()
        {
            return $"Hole => X={X}, Y={Y}, Diameter={Drill};";
        }
    }
}
