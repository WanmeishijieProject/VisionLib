using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Vision.ProcessStep
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
        public List<Tuple<HTuple, HTuple, HTuple, HTuple>> Out_Lines { get; private set; }

        public override bool Process()
        {
            try
            {
                HTuple ModelPos = new HTuple();
                ModelPos[0] = In_ModelRow;
                ModelPos[1] = In_ModelCOl;
                ModelPos[2] = In_ModelPhi;
                bool bRet = HalconVision.Instance.FindLineBasedModelRoi(In_Image, In_LineRoiPara, (HTuple)In_Hom_mat2D, ModelPos, out List<object> lineList);   //只需要显示
                if (Out_Lines == null)
                    Out_Lines = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                if (bRet && lineList != null && lineList.Count > 0)
                {
                    foreach (var it in lineList)
                    {
                        Out_Lines.Add(it as Tuple<HTuple, HTuple, HTuple, HTuple>);
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
