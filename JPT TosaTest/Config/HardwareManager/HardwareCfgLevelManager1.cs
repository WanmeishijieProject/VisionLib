using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{
    public class HardwareCfgLevelManager1
    {
        public bool Enabled { get; set; }
        public string InstrumentName { get; set; }
        public string ConnectMode { get; set; }
        public string PortName { get; set; }
    }
    public class PowerMeteConfig : HardwareCfgLevelManager1
    {

    }

 
 
    public class ComportCfg
    {
        public string PortName { get; set; }
        public string Port { get; set; }
        public int BaudRate { get; set; }
        public string Parity { get; set; }
        public int DataBits { get; set; }
        public int StopBits { get; set; }
        public int TimeOut { get; set; }
    }
 
    public class EtherNetCfg
    {
        public string PortName { get; set; }
        public string Mode { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string EOL { get; set; }
        public int TimeOut { get; set; }
    }
    public class GPIBConfig
    {
        public string PortName { get; set; }
        public int BoardAddress { get; set; }
        public int Address { get; set; }
    }
    public class NIVasaCfg
    {
        public string PortName { get; set; }
        public string KeyWord1 { get; set; }
        public string KeyWord2 { get; set; }
    }

}
