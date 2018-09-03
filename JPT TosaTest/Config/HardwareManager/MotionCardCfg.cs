using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{
    //为所有的运动控制卡提供配置
    public class MotionCardCfg
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public bool NeedInit { get; set; }
        public int MinAxisNo { get; set; }
        public int MaxAxisNo { get; set; }
        public string RealAxisNo { get; set; }
        public string SN { get; set; }
        public string ConnectMode { get; set; }
        public string PortName { get; set; }
    }
}
