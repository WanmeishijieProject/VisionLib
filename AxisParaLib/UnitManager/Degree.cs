using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxisParaLib.UnitManager
{
    public class Degree : UnitBase
    {
        public override double Factor => 1;

        public override string DisplayNameEN => "deg";

        public override string DisplayNameCH => "度";

        public override EnumUnitCategory Category => EnumUnitCategory.Angle;

        public override byte DecimalPoint => 6;

    }
}
