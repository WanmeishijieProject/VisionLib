using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionLib.CommonVisionStep
{
    public class StepFindeLineByModel : VisionProcessStepBase
    {


        public object In_Hom_mat2D { get; set; }
        public double In_ModelRow { get; set; } 
        public double In_ModelCOl { get; set; }
        public double In_ModelPhi { get; set; }
        public List<string> In_LineRoiPara { get; set; }

        /// <summary>
        /// 输出的直线
        /// </summary>
        public List<VisionLineData> Out_Lines { get; private set; }

        public override bool Process()
        {
            try
            {
                HTuple ModelPos = new HTuple();
                ModelPos[0] = In_ModelRow;
                ModelPos[1] = In_ModelCOl;
                ModelPos[2] = In_ModelPhi;
                bool bRet = HalconVision.Instance.FindLineBasedModelRoi(In_Image, In_LineRoiPara, (HTuple)In_Hom_mat2D, ModelPos, out List<VisionLineData> lineList);   //只需要显示
                if (Out_Lines == null)
                    Out_Lines = new List<VisionLineData>();
                if (bRet && lineList != null && lineList.Count > 0)
                {
                    foreach (var it in lineList)
                    {
                        Out_Lines.Add(it);
                    }
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
