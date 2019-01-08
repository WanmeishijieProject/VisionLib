using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionLib
{
    public class VisionDefinitions
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
            ShapeModel,
            ShapeModelXLD
        };  
        
        public enum EnumLinePolarityType
        {
            DarkToLight,
            LightToDark,
            All,
        }
        public enum EnumPairPolarityType
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
        public enum EnumRoiType
        {
            POINT,
            LINE,
            CIRCLE,
            RECTANGLE1,
            RECTANGLE2,
        }
        public enum EnumVisionColor
        {
            red,
            green,
            yellow,
            white,
            black,
            blue,
        }
        public enum EnumVisionDrawMode
        {
            margin,
            fill,
        }
    }
}
