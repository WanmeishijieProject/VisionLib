using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxisParaLib.UnitManager
{
    public class Radian : UnitBase
    {
        public override double Factor => 1;

        public override string DisplayNameEN => "rad";

        public override string DisplayNameCH => "弧度";

        public override EnumUnitCategory Category => EnumUnitCategory.Angle;
    }
}
