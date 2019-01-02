using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionLib.CommonVisionStep
{
    public class StepCalibImage : VisionProcessStepBase
    {
        public VisionLineData In_Line1 { get; set; }
        public VisionLineData In_Line2 { get; set; }
        public double In_RealDistance { get; set; }

        /// <summary>
        /// Pixels/um， Width方向
        /// 一个um对应的像素Width方向
        /// </summary>
        public double Out_PixGainFactorX { get; set; }
        /// <summary>
        /// Pixels/um， Height方向
        /// 一个um对应的像素Height方向
        /// </summary>
        public double Out_PixGainFactorY { get; set; }
        public override bool Process()
        {
            List<VisionLineData> list = new List<VisionLineData>();
            list.Add(In_Line1);
            list.Add(In_Line2);
            bool bRet= HalconVision.Instance.SetKValueOfCam(In_CamID, In_RealDistance, list, out double factor);
            Out_PixGainFactorX= Out_PixGainFactorY = factor;
            return bRet;

        }
    }
}
