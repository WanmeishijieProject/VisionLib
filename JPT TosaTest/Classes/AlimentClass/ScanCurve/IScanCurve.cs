using System.Collections;
using System.Windows;
using System.Windows.Media.Media3D;

namespace JPT_TosaTest.Classes.AlimentClass.ScanCure
{
    public interface IScanCurve : IList
    {
        string DisplayName { set; get; }
        string Prefix { set; get; }
        string Suffix { set; get; }

        Point FindMaximalPosition();
        Point3D FindMaximalPosition3D();
    }
}
