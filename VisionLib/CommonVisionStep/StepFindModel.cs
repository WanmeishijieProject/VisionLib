using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
namespace VisionLib.CommonVisionStep
{
    public class StepFindModel : VisionProcessStepBase
    {
        public string In_ModelNameFullPath { get; set; }

        /// <summary>
        /// 输出
        /// </summary>
        public HTuple Out_Hom_mat2D { get; private set; }
        public HTuple Out_ModelRow { get; private set; }
        public HTuple Out_ModelCol { get; private set; }
        public HTuple Out_ModelPhi { get; private set; }

        public override bool Process()
        {
            try
            {
                bool bRet = false;
                bRet=HalconVision.Instance.FindModelAndGetData(In_Image, In_ModelNameFullPath, out HTuple home_mat2D, out HTuple ModelPos);
                if (bRet && ModelPos.Length == 3)
                {
                    Out_Hom_mat2D = home_mat2D;
                    Out_ModelRow = ModelPos[0];
                    Out_ModelCol = ModelPos[1];
                    Out_ModelPhi = ModelPos[2];
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
