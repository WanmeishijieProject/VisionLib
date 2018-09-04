using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.PackageParseMgr.IrixiCommand
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
}
