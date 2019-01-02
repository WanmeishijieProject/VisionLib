using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionLib
{
    public class CameraInfoModel
    {
        public CameraInfoModel()
        {
            AttachedWindowDic = new Dictionary<string, HTuple>();
        }
        public object VisionLock => new object();
        public HObject Image { get; set; }
        public int CamID { get; set; }
        public string CamName { get; set; }
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
