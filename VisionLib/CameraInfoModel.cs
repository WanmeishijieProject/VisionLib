using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VisionLib.VisionDefinitions;

namespace VisionLib
{
    public class CameraInfoModel
    {
        public CameraInfoModel()
        {
            AttachedWindowDic = new Dictionary<string, HTuple>();
            CamID = -1;
        }
        public object VisionLock => new object();
        /// <summary>
        /// 通常是用户自己定义的名称
        /// </summary>
        public string ActualName { get; set; }
        /// <summary>
        /// 打开相机用到的参数
        /// </summary>
        public string NameForVision { get; set; }
        /// <summary>
        /// 相机的类型
        /// </summary>
        public EnumCamType Type { get; set; }
        public HObject Image { get; set; }
        public int CamID { get; set; }
        public HTuple AcqHandle { get; set; }
        public HTuple KX { get; set; }
        public HTuple KY { get; set; }
        public Dictionary<string,HTuple> AttachedWindowDic { get; set; }
        public bool IsActive { get; set; }
        public bool IsConnected { get; set; }
        public HTuple ImageWidth { get; set; }
        public HTuple ImageHeight { get; set; }
    }
}
