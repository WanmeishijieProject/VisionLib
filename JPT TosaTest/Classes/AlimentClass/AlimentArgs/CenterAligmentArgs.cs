using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPT_TosaTest.Classes.AlimentClass.ScanCure;

namespace JPT_TosaTest.Classes.AlimentClass.AlimentArgs
{
    public class CenterAligmentArgs : IAligmentArgs
    {
        public CenterAligmentArgs()
        {
            ArgsName = GetType().Name;
            Scg = new ScanCurveGroup() { new ScanCurve2D() {  DisplayName="曲线1"} };
        }
        public string ArgsName { get; }

        [Browsable(false)]
        public ScanCurveGroup Scg { get; }
    }
}
