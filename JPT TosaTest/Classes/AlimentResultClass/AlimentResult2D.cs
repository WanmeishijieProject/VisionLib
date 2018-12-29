using M12.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.AlimentResultClass
{
    public class AlimentResult2D : AlimentResultBase<Point2D>
    {
        public override Point2D GetMaxPoint2D(bool IsMax=true)
        {
            if (IsMax)
            {
                var data = DataList.OrderByDescending(pt => pt.Y);
                if (data.Count() != 0)
                    return data.First();
                return null;
            }
            else
            {
                var data = DataList.OrderByDescending(pt => pt.Y);
                if (data.Count() != 0)
                    return data.Last();
                return null;
            }
        }
    }
}
