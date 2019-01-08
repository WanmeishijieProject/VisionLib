using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionLib.DataModel
{
    public class VisionPointAngleData
    {
        public HTuple Row { get; set; }
        public HTuple Col { get; set; }
        public HTuple Angle { get; set; }
    }
}
