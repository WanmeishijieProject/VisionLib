using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{ 
    public class EthernetCfg : ICommunicationPortCfg
    {
        public string PortName { get; set; }
        public string Mode { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string EOL { get; set; }
        public int TimeOut { get; set; }
        public EnumConnectType GetTypeString()
        {
            return EnumConnectType.Ethernet;
        }
    }
}
