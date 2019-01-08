using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionLib.DataModel
{
    public class VisionRectangle1Data
    {
        public HTuple RowLT { get; set; }
        public HTuple ColLT { get; set; }
        public HTuple RowRB { get; set; }
        public HTuple ColRB { get; set; }
    }
}
