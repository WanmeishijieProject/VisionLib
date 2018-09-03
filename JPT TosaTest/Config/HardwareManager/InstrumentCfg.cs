using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{
    //为所有的Instrument提供配置
    public class InstrumentCfg
    {
        public bool Enabled { get; set; }
        public string InstrumentName { get; set; }
        public string ConnectMode { get; set; }
        public string PortName { get; set; }
    }
}
