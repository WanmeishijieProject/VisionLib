
using JPT_TosaTest.Config.SoftwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow
{
    public class WorkCalib : WorkFlowBase
    {
        protected override bool UserInit()
        {
            return true;
        }
        public WorkCalib(WorkFlowConfig cfg) : base(cfg)
        {
        }
        protected override int WorkFlow()
        {
          
            return 0;
        }

    }
}
