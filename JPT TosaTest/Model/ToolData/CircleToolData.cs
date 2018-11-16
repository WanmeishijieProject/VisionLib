using JPT_TosaTest.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Model.ToolData
{
    public class CircleToolData : ToolDataBase
    {
        public override EnumToolType ToolType { get { return EnumToolType.CircleTool; } set => throw new NotImplementedException(); }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
