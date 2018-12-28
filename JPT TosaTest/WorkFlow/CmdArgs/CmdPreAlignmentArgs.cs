using JPT_TosaTest.Config.ProcessParaManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow.CmdArgs
{
    public enum EnumPreAlignPolarity
    {
        LEFT,
        RIGHT,
    }
    public class CmdPreAlignmentArgs : CmdArgsBase
    {
        public EnumPreAlignPolarity PreAlignPolarity { get; set; }
        public BlindSearchArgsF BlindSearchArgs { get; set; }
    }
}
