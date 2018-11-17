using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPT_TosaTest.Vision;

namespace JPT_TosaTest.Model.ToolData
{
    public class LineToolData : ToolDataBase
    {

        public override EnumToolType ToolType { get { return EnumToolType.LineTool; } set { } }
        public int CaliperNum { get; set; }
        public EnumEdgeType Polarity { get; set; }
        public EnumSelectType SelectType { get; set; }
        public int Contrast { get; set; }
        public string ModelName { get; set; }

        public double RoiRow { get; set; }
        public double RoiCol { get; set; }
        public double RoiPhi { get; set; }
        public double RoiL1 { get; set; }
        public double RoiL2 { get; set; }

        public override string ToString()
        {
            return $"{ToolType.ToString()}|{CaliperNum}&{Polarity.ToString()}&{SelectType.ToString()}&{Contrast}&{ModelName}";
        }
        
    }
}
