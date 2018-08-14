using System;
using IrixiStepperControllerHelper;
using JPT_TosaTest.Config.HardwareManager;

namespace JPT_TosaTest.IOCards
{
    public class IO_IrixiEE0017 : IOBase
    {
        private IrixiMotionController _controller = null;
        public override bool Deinit()
        {
            return true;
        }

        public override bool Init(IOCardCfg ioCfg)
        {
            _controller = new IrixiMotionController();
            return true;
        }

        public override bool ReadIoInBit(int Index)
        {
            throw new NotImplementedException();
        }

        public override int ReadIoInWord(int StartIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override bool ReadIoOutBit(int Index)
        {
            throw new NotImplementedException();
        }

        public override int ReadIoOutWord(int StartIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override bool WriteIoOutBit(int Index, bool value)
        {
            return _controller.SetGeneralOutput(Index, value? OutputState.Enabled : OutputState.Disabled);
        }

        public override bool WriteIoOutWord(int StartIndex, ushort value)
        {
            throw new NotImplementedException();
        }
    }
}
