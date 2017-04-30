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
    public class FeederDefinition
    {
        public FeederDefinition()
        {
            Rows = new List<TrayRow>();
        }

        public IEnumerable<TrayRow> Rows { get; private set; }

        public Vector2 Size { get; set; }

        public bool IsStatus { get; set; }
    }

    public class TrayRow
    {
        double XOffset { get; set; }

        double Width { get; set; }
    }
}
