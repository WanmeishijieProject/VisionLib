using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{
    public class ComportCfg : ICommunicationPortCfg
    {
        public string PortName { get; set; }
        public string Port { get; set; }
        public int BaudRate { get; set; }
        public string Parity { get; set; }
        public int DataBits { get; set; }
        public int StopBits { get; set; }
        public int TimeOut { get; set; }

        public EnumConnectType GetTypeString()
        {
            return EnumConnectType.Comport;
        }
    }
}
