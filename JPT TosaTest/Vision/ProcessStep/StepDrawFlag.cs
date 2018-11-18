using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Vision.ProcessStep
{
    public class StepDrawFlag : VisionProcessStepBase
    {
        public Tuple<double, double, double, double> In_VLine { get; set; }
        public Tuple<double, double, double, double> In_HLine { get; set; }
        public HTuple In_WIndowHandle { get; set; }
        public EnumGeometryType In_Geometry { get; set; }
      
        public override bool Process()
        {
           
            HalconVision.Instance.DrawGeometry(In_WIndowHandle, In_Image, In_VLine.Item1, In_VLine.Item2, In_VLine.Item3, In_VLine.Item4,
                                        In_HLine.Item1, In_HLine.Item2, In_HLine.Item3, In_HLine.Item4, In_Geometry);
            return true;
        }
    }
}
