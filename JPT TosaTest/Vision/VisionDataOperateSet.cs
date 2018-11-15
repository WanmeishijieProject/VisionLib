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
        FlagTool,
    }
    public enum Enum_REGION_OPERATOR { ADD, SUB }
    public enum Enum_REGION_TYPE { RECTANGLE, CIRCLE }
    public enum EnumCamSnapState
    {
        IDLE,
        BUSY,
        DISCONNECTED

    }
    public enum EnumCamType
    {
        GigEVision,
        DirectShow,
        uEye,
        HuaRay
    }
    public enum EnumImageType
    {
        Window,
        Image
    }
    public enum EnumShapeModelType
    {
        Gray,
        Shape,
        XLD
    };
    public enum EnumRoiType
    {
        ModelRegionReduce,
    }
    public enum EnumEdgeType
    {
        DarkToLight,
        LightToDark,
        All,
    }
    public enum EnumPairType
    {
        Dark,
        Light,
        All,
    }
    public enum EnumSelectType
    {
        First,
        Last,
        All,
    }
    public enum EnumGeometryType
    {
        POINT,
        LINE,
        CIRCLE,
        RECTANGLE1,
        RECTANGLE2,
    }
    public class VisionDataOperateSet
    {

        private string _lineRectData = "";
        private string _pairRectData = "";
        private HObject _geometryData = new HObject();
        private HTuple _geometryPose = new HTuple();
        //private HTuple 
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
            switch (ToolType)
            {
                case EnumToolType.LineTool:
                    _lineRectData = "";
                    break;
                case EnumToolType.PairTool:
                    _pairRectData = "";
                    break;
                default:
                    break;
            }
        }
        public void ClearGeometryData()
        {
            _geometryData=null;
        }

        public string LineRoiData
        {
            get { return _lineRectData; }

        }
        public string PairRoiData
        {
            get { return _pairRectData; }
        }
        public HObject GeometryRegion
        {
            get { return _geometryData; }
            set { }
        }
        public HTuple GeometryPose
        {
            get { return _geometryPose; }
            set { }
        }
        public string GeometryPosString
        {
            get {
                if (GeometryPose.Length == 3)
                    return $"{GeometryPose[0]}&{GeometryPose[1]}&{GeometryPose[2]}";
                else
                    return "";    
            }
        }
    }
}
