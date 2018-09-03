using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards
{
    public enum EnumHomeType
    {
        O
    }
    public class AxisArgs
    {
        public AxisArgs()
        {
            CurAbsPos = 0;
            IsHomed = false;
            IsBusy = false;
            GainFactor = 1;
            HomeType = EnumHomeType.O;
            AxisLock = new object();
        }
        public double CurAbsPos { get; set; }
        public bool IsHomed { get; set; }
        public bool IsBusy { get; set; }
        public byte ErrorCode { get; set; }
        public int GainFactor { get; set; }
        public EnumHomeType HomeType { get; }
        public object AxisLock { get; }
    }
}
