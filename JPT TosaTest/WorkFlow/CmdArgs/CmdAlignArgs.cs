using JPT_TosaTest.Classes.AlimentResultClass;
using JPT_TosaTest.Config.ProcessParaManager;
using M12.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JPT_TosaTest.WorkFlow.CmdArgs
{
    public class CmdAlignArgs : CmdArgsBase
    {
        public event EventHandler<List<Point3D>> OnAligmentFinished;

        /// <summary>
        /// 确定是哪一边耦合
        /// </summary>
        public BlindSearchArgsF HArgs { get; set; }

        /// <summary>
        /// 正式耦合的参数
        /// </summary>
        public BlindSearchArgsF VArgs { get; set; }

        public void FireFinishAlimentEvent()
        {
            OnAligmentFinished?.Invoke(this, QResult);
        }

        public List<Point3D> QResult { get; set; }
    }
}
