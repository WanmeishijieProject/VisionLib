using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxisParaLib.UnitManager
{
    public enum EnumUnitCategory
    {
        Length,
        Angle,
    }
    public abstract class UnitBase
    {
        public abstract double Factor { get; }
        public abstract string DisplayNameEN { get; }
        public abstract string DisplayNameCH { get; }
        public abstract EnumUnitCategory Category {get;}
        public abstract byte DecimalPoint { get; }
    }
}
