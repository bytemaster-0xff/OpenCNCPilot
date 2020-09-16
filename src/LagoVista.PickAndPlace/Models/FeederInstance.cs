using LagoVista.Core.Models.Drawing;
using System;
using LagoVista.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace LagoVista.PickAndPlace.Models
{
    public class FeederInstance
    {
        public FeederInstance()
        {
            Rows = new List<Row>();
        }

        public Feeder Feeder
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Point2D<Double> Location
        {
            get;
            set;
        }

        public double Angle
        {
            get; set;
        }


        public void AdvancePart(int row)
        {
            Rows[row].CurrentPartIndex++;
        }

        public void SetFeeder(Feeder feeder)
        {
            Feeder = feeder;
            Name = Feeder.Name;
            for (var idx = 0; idx < Feeder.NumberRows; ++idx)
            {
                Rows.Add(new Row()
                {
                    RowNumber = idx + 1,
                });
            }
        }

        public List<Row> Rows { get; set; }


        public int ArucoId { get; set; }

        //   http://docs.opencv.org/trunk/db/da9/tutorial_aruco_board_detection.html
        //http://terpconnect.umd.edu/~jwelsh12/enes100/markergen.html

    }
}
