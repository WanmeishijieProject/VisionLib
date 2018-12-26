
using JPT_TosaTest.Config.SoftwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPT_TosaTest.Vision;
using JPT_TosaTest.MotionCards;
using JPT_TosaTest.IOCards;

namespace JPT_TosaTest.WorkFlow
{
    public class WF_Aligner : WorkFlowBase
    {
        public enum STEP : int
        {
            Init,
            HomeY,
            WaitHomeYOk,
            Home_X_Z_R_CX,
            WaitHomeOk,

            MoveX_To_Aligment,
            Wait_MoveX_To_Aligment,
            MoveY_To_Aligment,
            Wait_MoveY_To_Aligment,
            StartAlignment,
            ReadMem,
            DO_NOTHING,
            EXIT,
        }
        private Motion_IrixiEE0017 motion = null;
        private IO_IrixiEE0017 io = null;
        protected override bool UserInit()
        {
            motion = MotionMgr.Instance.FindMotionCardByAxisIndex(1) as Motion_IrixiEE0017;
            io = IOCardMgr.Instance.FindIOCardByCardName("IO_IrixiEE0017[0]") as IO_IrixiEE0017;
            bool bRet = motion != null && io != null;
            if (!bRet)
                ShowInfo($"初始化失败");
            return bRet;
        }
        public WF_Aligner(WorkFlowConfig cfg) : base(cfg)
        {

        }
        protected override int WorkFlow()
        {
            try
            {
                ClearAllStep();
                PushStep(STEP.Init);
                while (!cts.IsCancellationRequested)
                {
                    Thread.Sleep(10);
                    if (bPause)
                        continue;
                    Step = PeekStep();
                    switch (Step)
                    {
                        case STEP.Init:
                            ShowInfo("连续采集中......");
                            break;
                        case STEP.EXIT:
                            return 0;
                        default:
                            break;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                ShowInfo(ex.Message);
                ShowError(ex.Message);
                return -1;
            }
        }

    }
}
