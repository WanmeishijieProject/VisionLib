using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxisParaLib.UnitManager
{
    public class UnitHelper
    {
        public static double ConvertUnit(UnitBase FromUnit, UnitBase ToUnit, double value, int DecimalPoint)
        {
            if (FromUnit.Category == ToUnit.Category)
                return Math.Round(value * (ToUnit.Factor / FromUnit.Factor), DecimalPoint);
            else
                throw new Exception($"Can't trans unit from {FromUnit.DisplayNameEN} to {ToUnit.DisplayNameEN}");
        }

    }
}
