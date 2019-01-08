using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionLib.DataModel
{
    public class VisionLineData
    {
        public VisionLineData(HTuple RowStart, HTuple ColStart, HTuple RowEnd, HTuple ColEnd)
        {
            this.RowStart = RowStart;
            this.ColStart = ColStart;
            this.RowEnd = RowEnd;
            this.ColEnd = ColEnd;
        }
        public HTuple RowStart { get; set; }
        public HTuple ColStart { get; set; }
        public HTuple RowEnd { get; set; }
        public HTuple ColEnd { get; set; }
        public HTuple K
        {
            get { return (ColEnd - ColStart).D / (RowEnd - RowStart).D; }
        }
    }
}