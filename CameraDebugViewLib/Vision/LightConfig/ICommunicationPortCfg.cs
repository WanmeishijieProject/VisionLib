using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraDebugLib.Vision.LightConfig
{
    public enum EnumConnectType
    {
        Comport,
        Ethernet,
        GPIB,
        NIVisa
    }
    public interface ICommunicationPortCfg
    {
        string PortName { get; set; }
        EnumConnectType GetTypeString();
    }
}
