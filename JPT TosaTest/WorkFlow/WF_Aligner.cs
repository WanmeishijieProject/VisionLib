
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
using JPT_TosaTest.Model;
using M12.Definitions;
using M12.Base;

namespace JPT_TosaTest.WorkFlow
{
    public class WF_Aligner : WorkFlowBase
    {
        public enum STEP : int
        {
            Init,
            HomeAll,
            DoAlign,

            DO_NOTHING,
            EXIT,
        }


        private Motion_IrixiEE0017 motion = null;
        private IO_IrixiEE0017 io = null;
        private const int AXIS_X = 0, AXIS_Y = 1, AXIS_Z = 2, AXIS_R = 3, AXIS_CX = 4;

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
                            HomeAll();
                            MoveToInitPos();
                            PopStep();
                            break;

                        case STEP.HomeAll:
                            HomeAll();
                            PopStep();
                            break;
                        case STEP.DoAlign:
                            DoAlignment();
                            PopStep();
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

        #region Private Method
        private void HomeAll()
        {
            
            nSubStep = 1;
            while (!cts.IsCancellationRequested)
            {
                switch (nSubStep)
                {
                    case 1:
                        ShowInfo("Z轴回原点");
                        motion.Home(AXIS_Z, 0, 500, 1, 2);
                        nSubStep = 2;
                        break;
                    case 2:
                        if (motion.IsHomeStop(AXIS_Z))
                        {
                            ShowInfo("Y轴回原点");
                            motion.Home(AXIS_Y, 0, 500, 1, 2);
                            nSubStep = 3;
                        }
                        break;
                    case 3:
                        if (motion.IsHomeStop(AXIS_Y))
                        {
                            ShowInfo("X,R,CX轴回原点");
                            motion.Home(AXIS_X,0,500,1,2);
                            motion.Home(AXIS_R, 0, 500, 5, 10);
                            motion.Home(AXIS_CX, 0, 500, 20, 50);
                            nSubStep = 4;
                        }
                        break;
                    case 4:
                        if (motion.IsHomeStop(AXIS_X) && motion.IsHomeStop(AXIS_R) && motion.IsHomeStop(AXIS_CX))
                        {
                            ShowInfo("回原点完成");
                            return;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        private void MoveToInitPos()
        {
            nSubStep = 1;
            while (!cts.IsCancellationRequested)
            {
                switch (nSubStep)
                {
                    case 1:
                        break;
                    case 2:
                        break;
                }
            }
        }
        private void DoAlignment()
        {
            motion.DoBlindSearch(UnitID.U1, UnitID.U2, 0.1, 0.01, 5, 10001, ADCChannels.CH2, out List<Point3D> Value);

        }
        #endregion

    }
}
