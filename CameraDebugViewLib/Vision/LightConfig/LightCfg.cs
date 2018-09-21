using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraDebugLib.Vision.LightConfig
{
   
    public class LightCfg
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public bool NeedInit { get; set; }
        public int MinChannelNo { get; set; }
        public int MaxChannelNo { get; set; }
        public string ConnectMode { get; set; }
        public string PortName { get; set; }
    }

}
