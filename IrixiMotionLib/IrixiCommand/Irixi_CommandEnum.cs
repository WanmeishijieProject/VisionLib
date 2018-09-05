using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
   
    public enum EnumApiType : byte
    {
        CMD = 0x4D,
        DATA = 0x69
    }

    public enum Enumcmd : byte
    {
        Home,
        Move,
        MoveTrigOut,
        MoveTrigAdc,
        Stop,
        GetMcsuSta,
        ClearMcsuErr,
        ClearSysErr,
        GetSysSta,
        GetMemLength,
        ReadMem,
        ClearMem,
        SetDout,
        ReadDout,
        ReadDin,
        ReadAd,
        ConfigAdcTrigger,
        RunBlndSerach,
    }

    public enum EnumTriggerType
    {
        ADC,
        OUT,
    }

    public class MCSUS_STATE
    {
        public MCSUS_STATE()
        {
            AxisIndex = 0;
            IsInit = false;
            IsHomed = false;
            IsBusy = false;
            IsReversed = false;
            AbsPosition = 0;
            Acceleration = 0;
            Error = 0;
        }
        public byte AxisIndex { get; set; }
        public bool IsInit { get; set; }                   ///< Indicate whether the MCSU has been initialized successfully, 0:Not Init 1:Init
        public bool IsHomed { get; set; }                   ///< Indicate whether the MCSU has been homed, 0:Not Homed 1:Homed
        public bool IsBusy { get; set; }                   ///< Indicate whether the MCSU is busy, 0:Idle 1:Busy
        public bool IsReversed { get; set; }                ///< Indicate whether the direction of the MCSU is reversed, 0:Not Reversed 1:Reversed
        public Int32 AbsPosition { get; set; }             ///< The absolute position(steps) of the MCSU
        public Int16 Acceleration { get; set; }           ///< The current acceleration
        public byte Error { get; set; }                     ///< The last error of the MCSU
    }


}
