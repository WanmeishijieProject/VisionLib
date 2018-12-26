using System;
using System.Collections.Generic;
using System.IO.Ports;
using JPT_TosaTest.Communication;
using JPT_TosaTest.Config.HardwareManager;
using M12;
using M12.Definitions;
using JPT_TosaTest.MotionCards;

namespace JPT_TosaTest.IOCards
{
    public class IO_IrixiEE0017 : IIO
    {
        private Comport comport = null;
        private Controller _controller = null;
        private UInt16? OutputValue=0;
        public IOCardCfg ioCfg { get; set; }

        public event IOStateChange OnIOStateChanged;
        private const int MIN_CHANNEL = 0, MAX_CHANNEL = 7;


        public bool Deinit()
        {
            if (comport != null)
                return comport.ClosePort();
            return false;
        }

        public bool Init(IOCardCfg ioCfg, ICommunicationPortCfg communicationPortCfg)
        {
            try
            {
                this.ioCfg = ioCfg;
                ComportCfg portCfg = communicationPortCfg as ComportCfg;
                comport = CommunicationMgr.Instance.FindPortByPortName(ioCfg.PortName) as Comport;
                if (comport == null)
                    return false;
                else
                {
                    _controller = M12Wrapper.CreateInstance(portCfg.Port, portCfg.BaudRate);
                    _controller.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void _controller_OnOutputStateChanged( object sender,UInt16? e)
        {
            if (e.HasValue)
            {
                OnIOStateChanged?.Invoke(this,EnumIOType.OUTPUT, (UInt16)OutputValue, (UInt16)e);
                OutputValue = e;
            }
        }
        private void _controller_OnInputStateChanged(object sender, UInt16? e)
        {
            if (e.HasValue)
            {
                OnIOStateChanged?.Invoke(this, EnumIOType.INPUT, (UInt16)OutputValue, (UInt16)e);
            }
        }

        //Input
        public  bool ReadIoInBit(int Index, out bool value)
        {
            value = false;
            var ret = _controller.ReadDIN();
            return true;
        }

        public  bool ReadIoInWord(out int value)
        {
            value = 0;
            var ret=_controller.ReadDIN();
            value += (ret.DIN1 == DigitalIOStatus.ON ? (1<<0) : 0);
            value += (ret.DIN2 == DigitalIOStatus.ON ? (1<<1) : 0);
            value += (ret.DIN3 == DigitalIOStatus.ON ? (1<<2) : 0);
            value += (ret.DIN4 == DigitalIOStatus.ON ? (1<<3) : 0);
            value += (ret.DIN5 == DigitalIOStatus.ON ? (1<<4) : 0);
            value += (ret.DIN6 == DigitalIOStatus.ON ? (1<<5) : 0);
            value += (ret.DIN7 == DigitalIOStatus.ON ? (1<<6) : 0);
            value += (ret.DIN8 == DigitalIOStatus.ON ? (1<<7) : 0);
            return true;
        }


        //Output
        public  bool ReadIoOutBit(int Index, out bool value)
        {
            value = false;
            if (Index < MIN_CHANNEL || Index > MAX_CHANNEL)
                return false;
            int RealIndex = Index + (int)DigitalOutput.DOUT1;
            if (Enum.IsDefined(typeof(DigitalOutput), RealIndex))
            {
                ReadIoOutWord(out int Data);
                return ((Data >> Index) & 0x01) == 1;
            }
            return false;
        }

        public  bool ReadIoOutWord(out int value)
        {
            value = 0;
            var ret = _controller.ReadDOUT();
            value += (ret.DIN1 == DigitalIOStatus.ON ? (1 << 0) : 0);
            value += (ret.DIN2 == DigitalIOStatus.ON ? (1 << 1) : 0);
            value += (ret.DIN3 == DigitalIOStatus.ON ? (1 << 2) : 0);
            value += (ret.DIN4 == DigitalIOStatus.ON ? (1 << 3) : 0);
            value += (ret.DIN5 == DigitalIOStatus.ON ? (1 << 4) : 0);
            value += (ret.DIN6 == DigitalIOStatus.ON ? (1 << 5) : 0);
            value += (ret.DIN7 == DigitalIOStatus.ON ? (1 << 6) : 0);
            value += (ret.DIN8 == DigitalIOStatus.ON ? (1 << 7) : 0);
            return true;
        }

        /// <summary>
        /// 写IO的时候需要更新Output状态
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public  bool WriteIoOutBit(int Index, bool value)
        {
            if (Index < MIN_CHANNEL || Index > MAX_CHANNEL)
                return false;
            int RealIndex = Index + (int)DigitalOutput.DOUT1;
            if (Enum.IsDefined(typeof(DigitalOutput), RealIndex))
            {
                _controller.SetDOUT((DigitalOutput)RealIndex, value ? DigitalIOStatus.ON : DigitalIOStatus.OFF);

                ReadIoOutWord(out int IoOutData);
                _controller_OnOutputStateChanged(this, (UInt16?)IoOutData);
                return true;
            }

            return false ;
        }

        public  bool WriteIoOutWord(int value)
        {
            bool bRet = true;
            for (int i = 0; i < 8; i++)
            {
                bRet &= WriteIoOutBit(i, ((value >> i) & 0x01) == 1);
            }
            return bRet;

        }
    }
}
