using JPT_TosaTest.Classes.AlimentClass.ScanCure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.AlimentClass.AlimentArgs
{
    public interface IAligmentArgs
    {
        string ArgsName { get; }


        [Browsable(false)]
        ScanCurveGroup Scg {get; }
    }
}
