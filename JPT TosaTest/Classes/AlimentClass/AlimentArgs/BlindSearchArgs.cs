using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPT_TosaTest.Classes.AlimentClass.ScanCure;

namespace JPT_TosaTest.Classes.AlimentClass.AlimentArgs
{
    public class BlindSearchArgs : IAligmentArgs
    {
        public BlindSearchArgs()
        {
            Scg = new ScanCurveGroup();
            Scg.Add(new ScanCurve3D() {
              DisplayName="曲面0"    
            });

            ArgsName = GetType().Name;
            X = "X";
            Y = "Y";
            RangeX = 0;
            RangeY = 0;
            Interval = 0;
            Speed = 0;
        }

        public string ArgsName { get;}

        /// <summary>
        /// X,Y轴的名称
        /// </summary>
        public string X { get; set; }
        public string Y { get; set; }

        public double RangeX { get; set; }
        public double RangeY { get; set; }

        public double Interval { get; set; }

        public double Speed { get; set; }


        //各个曲线
        [Browsable(false)]
        public ScanCurveGroup Scg { get;}        
    }
}
