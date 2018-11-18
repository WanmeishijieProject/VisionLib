using HalconDotNet;
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
        public string HalconData { get; set; }
        public double CenterRow { get; set; }
        public double CenterCol { get; set; }
        public double CenterRadius { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
        public override bool FromString(string ParaList)
        {
            throw new NotImplementedException();
        }
    }
}
