using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LagoVista.Core;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Defines the Physical Definition of a Parts Tray
    /// </summary>
    public class FeederDefinition
    {
        public string Id { get; set; }

        public FeederDefinition()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Name { get; set; }

        public double Width { get; set; }
        public double Height{ get; set; }

        public int Rows { get; set; }

        public double TapeWidth { get; set; }

        public double RowWidth { get; set; }

        public double FirstRowOffset { get; set; }

        public int ArucoId { get; set; }

        //   http://docs.opencv.org/trunk/db/da9/tutorial_aruco_board_detection.html
        //http://terpconnect.umd.edu/~jwelsh12/enes100/markergen.html


        public bool IsStatic { get; set; }
    }
}
