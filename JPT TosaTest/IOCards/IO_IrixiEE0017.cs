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
        private UInt16 OutputValue = 0;
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
                    _controller.OnOutputStateChanged += _controller_OnOutputStateChanged;
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

        private void _controller_OnOutputStateChanged(ushort OldValue, ushort NewValue)
        {
            OutputValue= NewValue;
        }

        //Input
        public  bool ReadIoInBit(int Index, out bool value)
        {
            value = false;
            return _controller.ReadIoInBit(Index+1, out value);
        }

        public  bool ReadIoInWord(int StartIndex, out int value)
        {
            value = 0;
            return _controller.ReadIoInWord(StartIndex+1, out value);
        }


        //Output
        public  bool ReadIoOutBit(int Index, out bool value)
        {
            value = false;
            return _controller.ReadIoOutBit(Index+1, out value);
        }

        public  bool ReadIoOutWord(int StartIndex, out int value)
        {
            value = 0;
            return _controller.ReadIoOutWord(StartIndex+1, out value);
        }

        public  bool WriteIoOutBit(int Index, bool value)
        {
            return _controller.WriteIoOutBit(Index+1, value);
        }

        public  bool WriteIoOutWord(int StartIndex, ushort value)
        {
            return _controller.WriteIoOutWord(StartIndex+1, value);
           
        }
    }
}
