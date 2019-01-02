using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VisionLib.CommonVisionStep
{
    public abstract class VisionProcessStepBase
    {

        public virtual int In_CamID { get; set; }
        public virtual HObject In_Image { get; set; } 
        public abstract bool Process();
    }
}
