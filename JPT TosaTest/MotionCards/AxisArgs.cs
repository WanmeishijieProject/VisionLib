using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards
{
    public enum EnumHomeType
    {
        Origin,
        P,
        N
    }
    public class AxisArgs
    {
        public AxisArgs()
        {
            CurAbsPos = 0;
            IsHomed = false;
            IsRunning = false;
            GainFactor = 1;
            HomeType = EnumHomeType.Origin;
            AxisLock = new object();
        }
       
        public double CurAbsPos { get; set; }
        public bool IsHomed { get; set; }
        public bool IsRunning { get; set; }
        public int GainFactor { get; set; }
        public EnumHomeType HomeType { get; }

        public object AxisLock { get; }
    }
}
