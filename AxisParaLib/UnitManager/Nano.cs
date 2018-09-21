using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxisParaLib.UnitManager
{
    public class Nano : UnitBase
    {

        public override double Factor => 1000000.0f;

        public override string DisplayNameEN => "nm";

        public override string DisplayNameCH => "纳米";

        public override EnumUnitCategory Category => EnumUnitCategory.Length;
    }
}
