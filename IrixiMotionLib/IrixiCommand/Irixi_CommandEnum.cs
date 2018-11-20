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
        HOST_CMD_HOME,
        HOST_CMD_MOVE,
        HOST_CMD_MOVE_T_OUT,
        HOST_CMD_MOVE_T_ADC,
        HOST_CMD_STOP,
        HOST_CMD_SET_ACC,
        HOST_CMD_SET_MODE,
        HOST_CMD_GET_SYS_INFO,
        HOST_CMD_GET_MCSU_STA,
        HOST_CMD_GET_MCSU_SETTINGS,
        HOST_CMD_GET_SYS_STA,
        HOST_CMD_GET_ERR,
        HOST_CMD_GET_MEM_LEN,
        HOST_CMD_READ_MEM,
        HOST_CMD_CLEAR_MEM,
        HOST_CMD_SET_DOUT,
        HOST_CMD_READ_DOUT,
        HOST_CMD_READ_DIN,
        HOST_CMD_READ_AD,
        HOST_CMD_EN_CSS,            //Enable 
        HOST_CMD_SET_CSSTHD,        //Set threshold
        HOST_CMD_SET_T_ADC,
        HOST_CMD_SET_T_OUT,
        HOST_CMD_BLINDSEARCH,
        HOST_CMD_SAV_MCSU_ENV,
        HOST_CMD_SYS_RESET
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
            Error = 0;
        }
        public byte AxisIndex { get; set; }
        public bool IsInit { get; set; }                   ///< Indicate whether the MCSU has been initialized successfully, 0:Not Init 1:Init
        public bool IsHomed { get; set; }                   ///< Indicate whether the MCSU has been homed, 0:Not Homed 1:Homed
        public bool IsBusy { get; set; }                   ///< Indicate whether the MCSU is busy, 0:Idle 1:Busy
        public bool IsReversed { get; set; }                ///< Indicate whether the direction of the MCSU is reversed, 0:Not Reversed 1:Reversed
        public Int32 AbsPosition { get; set; }             ///< The absolute position(steps) of the MCSU
        //public Int16 Acceleration { get; set; }           ///< The current acceleration
        public byte Error { get; set; }                     ///< The last error of the MCSU
    }


}
