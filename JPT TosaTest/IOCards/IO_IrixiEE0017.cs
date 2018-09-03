using System;
using System.Collections.Generic;
using System.IO.Ports;
using JPT_TosaTest.Communication;
using JPT_TosaTest.Config.HardwareManager;
using JPT_TosaTest.MotionCards;

namespace JPT_TosaTest.IOCards
{
    public class IO_IrixiEE0017 : IIO
    {
        private Comport comport = null;
        private IrixiEE0017 _controller = null;

        public IOCardCfg ioCfg { get; set; }

        public bool Deinit()
        {
            if (comport != null)
                return comport.ClosePort();
            return false;
        }
        public bool Init(IOCardCfg ioCfg, ICommunicationPortCfg communicationPortCfg)
        {
            this.ioCfg = ioCfg;
            comport = CommunicationMgr.Instance.FindPortByPortName(ioCfg.PortName) as Comport;
            if (comport == null)
                return false;
            else
            {
                _controller = IrixiEE0017.CreateInstance(ioCfg.PortName);
                if (_controller != null)
                {
                    if (ioCfg.NeedInit)
                    {
                        return _controller.Init(Int32.Parse(comport.ToString().ToLower().Replace("com", "")));
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private void Comport_OnDataReceived1()
        {
            throw new NotImplementedException();
        }

        public  bool ReadIoInBit(int Index, out bool value)
        {
            throw new NotImplementedException();
        }

        public  bool ReadIoInWord(int StartIndex, out int value)
        {
            value = 0;
            return _controller.ReadIoInWord(StartIndex, out value);
        }

        public  bool ReadIoOutBit(int Index, out bool value)
        {
            throw new NotImplementedException();
        }

        public  bool ReadIoOutWord(int StartIndex, out int value)
        {
            value = 0;
            return _controller.ReadIoOutWord(StartIndex, out value);
        }

        public  bool WriteIoOutBit(int Index, bool value)
        {
            return _controller.WriteIoOutBit(Index, value);
        }

        public  bool WriteIoOutWord(int StartIndex, ushort value)
        {
            return _controller.WriteIoOutWord(StartIndex, value);
           
        }
    }
}
