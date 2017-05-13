﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public class Row
    {
        public int RowNumber { get; set; }

        public Part Part { get; set; }

        public string Display
        {
            get
            {
                if (Part != null)
                {
                    return $"{RowNumber}. {Part.Display}";
                }
                else
                {
                    return $"{RowNumber}.";
                }
            }
        }

    }
}
