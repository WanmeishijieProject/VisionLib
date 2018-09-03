using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{

    //位IO卡提供配置
    public class IOCardCfg
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public bool NeedInit { get; set; }
        public string ConnectMode { get; set; }
        public string IOName_Input { get; set; }
        public string IOName_Output { get; set; }
        public string PortName { get; set; }

    }
}
