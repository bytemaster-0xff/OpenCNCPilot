using LagoVista.Core.Models.Drawing;
using System;
using LagoVista.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public Vector3 GetCurrentPartLocation(int rowIndex)
        {
            var row = Rows[rowIndex];
            return new Vector3()
            {
                X = Location.X + row.CenterX,
                Y = Location.Y + row.FirstComponentY + row.DeltaY * row.CurrentPartIndex,
                Z = Feeder.PartZ
            };
        }

        public String CurrentPartGCode(int rowIndex)
        {
            var location = GetCurrentPartLocation(rowIndex);

            return $"G01 X{location.X.ToDim()} Y{location.Y.ToDim()}";
        }

        public void AdvancePart(int row)
        {
            Rows[row].CurrentPartIndex++;
        }

        public void SetFeder(Feeder feeder)
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
