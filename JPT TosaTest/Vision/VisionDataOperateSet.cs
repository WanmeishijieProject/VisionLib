using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
namespace JPT_TosaTest.Vision
{
    public enum EnumToolType
    {
        LineTool,
        CircleTool,
        PairTool,
    }
    public class VisionDataOperateSet
    {

        private string _lineRectData = "";
        private string _pairRectData = "";
        protected string GetRectData(EnumToolType ToolType, HTuple Row, HTuple Col, HTuple Pi, HTuple L1, HTuple L2)
        {
            switch (ToolType)
            {
                case EnumToolType.LineTool:
                    return _lineRectData = $"{Row.D}&{Col.D}&{Pi.D}&{L1.D}&{L2.D}";
                case EnumToolType.PairTool:
                    return _pairRectData = $"{Row.D}&{Col.D}&{Pi.D}&{L1.D}&{L2.D}";
                default:
                    return "";
            }
            
        }
        public void ClearRectData(EnumToolType ToolType)
        {
            _lineRectData = "";
        }
        public string LineRoiData
        {
            get { return _lineRectData; }

        }
        public string PairRoiData
        {
            get { return _pairRectData; }
        }
    }
}
