using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
namespace JPT_TosaTest.Vision.ProcessStep
{
    public class StepFindModel : VisionProcessStepBase
    {
        public string ModelNameFullPath { get; set; }


        /// <summary>
        /// 输出
        /// </summary>
        public HTuple Hom_mat2D { get; private set; }
        public HTuple ModelRow { get; private set; }
        public HTuple ModelCol { get; private set; }
        public HTuple ModelPhi { get; private set; }

        public override bool Process()
        {
            try
            {
                bool bRet = false;
                bRet=HalconVision.Instance.FindModelAndGetData(Image, ModelNameFullPath, out HTuple home_mat2D, out HTuple ModelPos);
                if (bRet && ModelPos.Length == 3)
                {
                    Hom_mat2D = home_mat2D;
                    ModelRow = ModelPos[0];
                    ModelCol = ModelPos[1];
                    ModelPhi = ModelPos[2];
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
