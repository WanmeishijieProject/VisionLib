using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{
    public class GpibCfg : ICommunicationPortCfg
    {
        public string PortName { get; set; }
        public int BoardAddress { get; set; }
        public int Address { get; set; }
        public EnumConnectType GetTypeString()
        {
            return EnumConnectType.GPIB;
        }
    }
}
