using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Defines the Physical Definition of a Parts Tray
    /// </summary>
    public class PartsTrayDefinition
    {
        public int Rows { get; set; }

        public Vector2 Size { get; set; }
    }
}
