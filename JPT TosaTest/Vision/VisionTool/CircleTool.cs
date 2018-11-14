using JPT_TosaTest.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Vision.VisionTool
{
    public class CircleTool : ToolBase
    {
        #region  Property
        public override string DefaultPath
        {
            get { return FileHelper.GetCurFilePathString() + @"VisionData\ToolData\"; }
            set { }
        }
        #endregion
    }
}
