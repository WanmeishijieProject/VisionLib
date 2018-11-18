using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Vision.ProcessStep
{
    public class StepCalibImage : VisionProcessStepBase
    {
        public Tuple<double, double, double, double> In_Line1 { get; set; }
        public Tuple<double, double, double, double> In_Line2 { get; set; }
        public double In_RealDistance { get; set; }


        public double Out_PixGainFactor { get; set; }
        public override bool Process()
        {
            List<object> list = new List<object>();
            list.Add(In_Line1);
            list.Add(In_Line2);
            bool bRet= HalconVision.Instance.SetKValueOfCam(In_CamID, In_RealDistance, list, out double factor);
            Out_PixGainFactor = factor;
            return bRet;

        }
    }
}
