using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JPT_TosaTest.Vision.ProcessStep
{
    public abstract class VisionProcessStepBase
    {
        public int CamID { get; set; }
        public HObject Image { get; set; } 
        public abstract bool Process();
        public double PixGainFactor { get; set; }
    }
}
