using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxisParaLib.UnitManager
{
    public class Millimeter : UnitBase
    {
        public override double Factor => 1;
        public override string DisplayNameEN => "mm";
        public override string DisplayNameCH => "毫米";
        public override EnumUnitCategory Category => EnumUnitCategory.Length;
        public override byte DecimalPoint => 6;
    }
}
