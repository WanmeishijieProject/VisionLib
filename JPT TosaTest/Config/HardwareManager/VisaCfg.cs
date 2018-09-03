using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{
    public class VisaCfg : ICommunicationPortCfg
    {
        public string PortName { get; set; }
        public string KeyWord1 { get; set; }
        public string KeyWord2 { get; set; }
        public EnumConnectType GetTypeString()
        {
            return EnumConnectType.NIVisa;
        }
    }

}
