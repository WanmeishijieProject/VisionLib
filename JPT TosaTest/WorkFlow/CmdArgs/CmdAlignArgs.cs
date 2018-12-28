using JPT_TosaTest.Config.ProcessParaManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow.CmdArgs
{
    public class CmdAlignArgs : CmdArgsBase
    {
        /// <summary>
        /// 确定是哪一边耦合
        /// </summary>
        public BlindSearchArgsF HArgs { get; set; }

        /// <summary>
        /// 正式耦合的参数
        /// </summary>
        public BlindSearchArgsF VArgs { get; set; }

    }
}
