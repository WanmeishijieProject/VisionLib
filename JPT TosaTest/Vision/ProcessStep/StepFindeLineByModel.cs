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

        public object Hom_mat2D { get; set; }
        public double ModelRow { get; set; } 
        public double ModelCOl { get; set; }
        public double ModelPhi { get; set; }
        public List<string> LineRoiPara { get; set; }

        /// <summary>
        /// 输出的直线
        /// </summary>
        public List<Tuple<HTuple, HTuple, HTuple, HTuple>> Lines { get; private set; }

        public override bool Process()
        {
            try
            {
                HTuple ModelPos = new HTuple();
                ModelPos[0] = ModelRow;
                ModelPos[1] = ModelCOl;
                ModelPos[2] = ModelPhi;
                bool bRet = HalconVision.Instance.FindLineBasedModelRoi(Image, LineRoiPara, (HTuple)Hom_mat2D, ModelPos, out List<object> lineList);   //只需要显示
                if (bRet && lineList != null && lineList.Count > 0)
                {
                    foreach (var it in lineList)
                    {
                        Lines.Add(it as Tuple<HTuple, HTuple, HTuple, HTuple>);
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
