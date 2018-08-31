using JPT_TosaTest.Config.HardwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Communication
{
    public abstract class CommunicationPortBase
    {
        public CommunicationPortBase(ICommunicationPortCfg portCfg) { }
        string PortName { get; set; }
        public abstract bool OpenPort();
        public abstract bool ClosePort();
        public abstract bool IsOpen();
    }
}
