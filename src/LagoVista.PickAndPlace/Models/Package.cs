using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public class Package
    {
        LagoVista.Core.Models.Drawing.Vector2 _size;

        public enum StandardSizes
        {
            Part0402,
            Part0603,
            Part0805,
            Part1206,
            Part1210
        }

        public Package()
        {

        }

        public void SetStandardSize(StandardSizes size)
        {
            switch(size)
            {
                case StandardSizes.Part0402: Size = new Vector2(1, 0.5); break;
                case StandardSizes.Part0603: Size = new Vector2(1.6, 0.8); break;
                case StandardSizes.Part0805: Size = new Vector2(2.0, 1.25); break;
                case StandardSizes.Part1206: Size = new Vector2(3.2, 1.6); break;
            }
        }

        public Vector2 Size { get; set; }
    }
}
