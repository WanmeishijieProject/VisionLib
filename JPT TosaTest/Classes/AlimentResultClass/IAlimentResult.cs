using M12.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.AlimentResultClass
{
    public interface IAlimentResult
    {
        Point2D GetMaxPoint2D(bool IsMax=true);
        Point3D GetMaxPoint3D(bool IsMax = true);
    }
}
