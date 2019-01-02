using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VisionLib.VisionDefinitions;

namespace VisionLib.CommonVisionStep
{
    public class StepDrawFlag : VisionProcessStepBase
    {
        private HObject _geomertyRegion = new HObject();
        private HTuple _poseOfRegion = new HTuple();

        public Tuple<double, double, double, double> In_VLine { get; set; }
        public Tuple<double, double, double, double> In_HLine { get; set; }
        public EnumRoiType In_Geometry { get; set; }
      

        public HObject Out_GeometryRegion
        {
            get { return _geomertyRegion; }
        }
        public HTuple Out_PoseOfRegion {
            get { return _poseOfRegion; }
        }
        public override bool Process()
        {
           
            HalconVision.Instance.DrawGeometry(In_CamID, In_Image, In_VLine.Item1, In_VLine.Item2, In_VLine.Item3, In_VLine.Item4,
                                        In_HLine.Item1, In_HLine.Item2, In_HLine.Item3, In_HLine.Item4, In_Geometry, out _geomertyRegion,out _poseOfRegion);
            return true;
        }
    }
}
