using M12.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.AlimentResultClass
{
    public class AlimentResult3D : AlimentResultBase<Point3D>
    {
        public string ZTitle { get; set; }
        public override Point3D GetMaxPoint3D(bool IsMax=true)
        {
            if (IsMax)
            {
                var data = DataList.OrderByDescending(pt => pt.Z);
                if (data.Count() != 0)
                    return data.First();
                return null;
            }
            else
            {
                var data = DataList.OrderByDescending(pt => pt.Z);
                if (data.Count() != 0)
                    return data.Last();
                return null;
            }
        }
    }
}
