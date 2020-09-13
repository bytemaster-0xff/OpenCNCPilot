using LagoVista.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public class PartPackSlot : ModelBase
    {
        private int _row;
        public int Row
        {
            get { return _row; }
            set { Set(ref _row, value); }
        }

        private int _column;
        public int Column
        {
            get { return _column; }
            set { Set(ref _column, value); }
        }

        private string _feederId;
        public string FeederId
        {
            get { return _feederId; }
            set { Set(ref _feederId, value); }
        }
    }
}
