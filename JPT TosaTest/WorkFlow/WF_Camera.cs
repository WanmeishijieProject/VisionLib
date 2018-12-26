
using JPT_TosaTest.Config.SoftwareManager;
using JPT_TosaTest.IOCards;
using JPT_TosaTest.MotionCards;
using JPT_TosaTest.MotionCards.IrixiCommand;
using JPT_TosaTest.Vision;
using M12.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow
{
    public class WF_Camera : WorkFlowBase
    {
        public enum STEP : int
        {
            Init,
            DO_NOTHING,
            EXIT,
        }
        private Motion_IrixiEE0017 motion = null;
        private IO_IrixiEE0017 io = null;
        private const int CAM_TOP = 0, CAM_BACK = 1;
        protected override bool UserInit()
        {
            try
            {
                motion = MotionMgr.Instance.FindMotionCardByAxisIndex(1) as Motion_IrixiEE0017;
                io = IOCardMgr.Instance.FindIOCardByCardName("IO_IrixiEE0017[0]") as IO_IrixiEE0017;
                bool bRet= motion!=null && io!=null;
                if (!bRet)
                    ShowInfo($"初始化失败");
                return true;

            }
            catch(Exception ex)
            {
                ShowError(ex.Message);
                return false;
            }
        }
        public WF_Camera(WorkFlowConfig cfg) : base(cfg)
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
                            HalconVision.Instance.GrabImage(CAM_TOP,true,true);
                            HalconVision.Instance.GrabImage(CAM_BACK,true,true);
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
            catch(Exception ex)
            {
                ShowInfo(ex.Message);
                ShowError(ex.Message);
                return -1;
            }
        }

   
    }
}
