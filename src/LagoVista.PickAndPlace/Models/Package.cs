using LagoVista.Core.Models;
using System;
using LagoVista.Core;

namespace LagoVista.PickAndPlace.Models
{
    public class Package : ModelBase
    {
        public Package()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        public string Name { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double TapeWidth { get; set; }
        public int RotationInTape { get; set; }
    }
}
