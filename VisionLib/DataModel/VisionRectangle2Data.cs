using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionLib.DataModel
{
    public class VisionRectangle2Data
    {
        public HTuple RowCenter { get; set; }
        public HTuple ColCenter { get; set; }
        public HTuple Phi { get; set; }
        public HTuple L1 { get; set; }
        public HTuple L2 { get; set; }
    }
}
