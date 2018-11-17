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
    public enum IMAGEPROCESS_STEP
    {
        T1, //Get model data 
        T2, //Get line bottom

        T3,  //Get line top

        T4, //Display raw line
        T5,//Display line offset

        T6, //找Tia模板
        T7, //显示Tia最终处理后的region

    }
    public class VisionDataOperateSet
    {

        private string _lineRectData = "";
        private string _pairRectData = "";
        private string _circleData = "";
        private HObject _geometryRegion;

        public VisionDataOperateSet()
        {
            HOperatorSet.GenEmptyObj(out HObject _geometryRegion);
               
        }
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
        public void ClearToolRoiData(EnumToolType ToolType)
        {
            switch (ToolType)
            {
                case EnumToolType.LineTool:
                    _lineRectData = "";
                    break;
                case EnumToolType.PairTool:
                    _pairRectData = "";
                    break;
                case EnumToolType.FlagTool:
                    if (_geometryRegion != null)
                    {
                        _geometryRegion.Dispose();
                        _geometryRegion = null;
                    }
                    HOperatorSet.GenEmptyObj(out _geometryRegion);
                    GeometryPose = new HTuple();
                    break;
                case EnumToolType.CircleTool:
                   
                    _circleData = "";
                    
                    break;
                default:
                    break;
            }
        }

        public string LineRoiData
        {
            get { return _lineRectData; }
            set { _lineRectData = value; }

        }
        public string PairRoiData
        {
            get { return _pairRectData; }
            set { _pairRectData = value; }
        }
        public HObject GeometryRegion
        {
            get { return _geometryRegion; }
            set { _geometryRegion = value; }
        }
        public HTuple GeometryPose
        {
            get;set;
        }

        /// <summary>
        /// Row, Col, Phi
        /// </summary>
        public string GeometryPosString
        {
            get {
                if (GeometryPose!=null && GeometryPose.Length == 3)
                    return $"{GeometryPose[0].D}&{GeometryPose[1].D}&{GeometryPose[2].D}";
                else
                    return "";    
            }
        }
        public string CircleRoiData
        {
            get { return _circleData; }
            set { _circleData = value; }
        }
    }
}
