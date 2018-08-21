
using JPT_TosaTest.Config.HardwareManager;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Instrument
{

    public struct COMPORT_DATA
    {
        public string PortName;
        public string Port;
        public int BaudRate;
        public System.IO.Ports.Parity parity;
        public int DataBits;
        public StopBits stopbits;
        public int Timeout;
    }
    public abstract class InstrumentBase
    {
        protected COMPORT_DATA comportData;
        protected InstrumentCfg Config = null;
        //Comport
        protected SerialPort comPort = null;
        public int Index = -1;
        protected object _lock = new object();
        public abstract bool Init();
        public abstract bool DeInit();
        public InstrumentBase(InstrumentCfg cfg)
        {
            Config = cfg;
        }
        protected void GetPortProfileData(ComportCfg comportCfg)
        {
            comportData.PortName = comportCfg.PortName;
            comportData.Port = comportCfg.Port;
            comportData.BaudRate = comportCfg.BaudRate;
            comportData.DataBits = comportCfg.DataBits;
            comportData.Timeout = comportCfg.TimeOut;
            switch (comportCfg.Parity.ToLower())
            {
                case "n":
                    comportData.parity = System.IO.Ports.Parity.None;
                    break;
                case "o":
                    comportData.parity = System.IO.Ports.Parity.Odd;
                    break;
                default:
                    comportData.parity = System.IO.Ports.Parity.Even;
                    break;
            }
            switch (comportCfg.StopBits)
            {
                case 0:
                    comportData.stopbits = StopBits.None;
                    break;
                case 1:
                    comportData.stopbits = StopBits.One;
                    break;
                case 2:
                    comportData.stopbits = StopBits.Two;
                    break;
                default:
                    comportData.stopbits = StopBits.OnePointFive;
                    break;
            }
        }
    }
}