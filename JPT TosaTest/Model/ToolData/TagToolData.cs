using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPT_TosaTest.Vision;

namespace JPT_TosaTest.Model.ToolData
{
    public class TagToolData : ToolDataBase
    {
        public override EnumToolType ToolType { get { return EnumToolType.FlagTool; } set => throw new NotImplementedException(); }
        public EnumGeometryType GeometryType { get; set; }
        public string L1Name { get; set; }
        public string L2Name { get; set; }

        public override string ToString()
        {
            return $"{ToolType.ToString()}|{GeometryType.ToString()}&{L1Name}&{L2Name}";
        }
    }
}
