using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxisParaLib.UnitManager
{
    public class Micron : UnitBase
    {
        public override double Factor => 1000;

        public override string DisplayNameEN => "μm";

        public override string DisplayNameCH => "微米";

        public override EnumUnitCategory Category => EnumUnitCategory.Length;
    }
}
