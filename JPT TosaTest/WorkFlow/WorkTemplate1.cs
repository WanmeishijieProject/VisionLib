
using JPT_TosaTest.Config.SoftwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow
{
    public class WorkTemplate1 : WorkFlowBase
    {
        protected override bool UserInit()
        {
            return true;
        }
        public WorkTemplate1(WorkFlowConfig cfg) : base(cfg)
        {

        }
        protected override int WorkFlow()
        {
            int i = 0;
            while (!cts.IsCancellationRequested)
            {
                Thread.Sleep(100);
                if (bPause)
                    continue;
                
                ShowInfo($"{i}{i}{i}{i++}");
                
            }
            return 0;
        }

    }
}
