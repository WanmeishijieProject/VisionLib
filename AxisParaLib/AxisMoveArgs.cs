using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxisParaLib
{
    public class AxisMoveArgs
    {
        public AxisMoveArgs()
        {
            Speed = 0;
            Distance = 0;
            MoveMode = 0;
        }
        public double Speed
        {
            get;
            set;
        }
        public double Distance { get; set; }
        public int MoveMode { get; set; }   //ABS, REL
    }
}
