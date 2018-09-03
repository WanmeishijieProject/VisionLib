
using JPT_TosaTest.Config.SoftwareManager;
using JPT_TosaTest.IOCards;
using JPT_TosaTest.MotionCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow
{
    public class WorkTemplate : WorkFlowBase
    {
        public enum STEP : int
        {
            Init,
            MoveAbs5000,
            WaitAbs5000Ok,
            MoveAbs1000,
            WaitAbs1000Ok,
            WaitHomeOk,
            Move1000,
            WaitMoveOk,
        }
        private Motion_IrixiEE0017 motion = null;
        private IO_IrixiEE0017 io = null;
        protected override bool UserInit()
        {
            motion = MotionMgr.Instance.FindMotionCardByAxisIndex(4) as Motion_IrixiEE0017;
            io = IOCardMgr.Instance.FindIOCardByCardName("IO_IrixiEE0017[0]") as IO_IrixiEE0017;
            //motion.Home(4,0,0,0,0);
            //motion.MoveRel(4, 0, 5000, 5000);
            //motion.MoveAbs(4, 0, 5000, 10000);
            //motion.SetTrigConfig(0xFF);
            //motion.ClearMem();
            //motion.MoveRel(4, 0, 5000, 1000, EnumTriggerType.ADC, 100);
            //motion.ClearMem();

            motion.GetMemLength(out UInt32 Len);
            motion.ReadMem(0, Len, out List<Int16> RawData);

            io.WriteIoOutBit(1, false);
            io.WriteIoOutBit(2, false);
            io.WriteIoOutBit(4, false);
            io.WriteIoOutBit(3, false);

            io.WriteIoOutBit(1, true);
            io.WriteIoOutBit(2, true);
            io.WriteIoOutBit(4, true);
            io.WriteIoOutBit(3, true);

            io.ReadIoOutWord(0, out int value);
           
            return false;
        }
        public WorkTemplate(WorkFlowConfig cfg) : base(cfg)
        {

        }
        protected override int WorkFlow()
        {
            ClearAllStep();
            PushStep(STEP.Init);
            int i = 0;
            int Dir = 1;
            while (!cts.IsCancellationRequested)
            {
                Thread.Sleep(10);
                if (bPause)
                    continue;
                nStep = PeekStep();
                switch (nStep)
                {
                    case STEP.Init:
                        motion.Home(4, 0, 0, 0, 0);
                        PopAndPushStep(STEP.WaitHomeOk);
                        ShowInfo();
                        break;
                    case STEP.WaitHomeOk:
                        if (motion.IsHomeStop(4))
                        {
                            PopAndPushStep(STEP.MoveAbs5000);
                        }
                        ShowInfo();
                        break;
                    case STEP.MoveAbs5000:
                        motion.MoveAbs(4, 0, 5000, 5000);
                        PopAndPushStep(STEP.WaitAbs5000Ok);
                        break;
                    case STEP.WaitAbs5000Ok:
                        if (motion.IsNormalStop(4))
                        {
                            motion.MoveAbs(4,0,1000,1000);
                            PopAndPushStep(STEP.WaitAbs1000Ok);
                        }
                        break;
                    case STEP.WaitAbs1000Ok:
                        if (motion.IsNormalStop(4))
                        {
                            motion.MoveAbs(4, 0, 1000, 1000);
                            PopAndPushStep(STEP.MoveAbs5000);
                        }
                        break;
               
                    case STEP.Move1000:
                        motion.MoveRel(4, 0, 10000, Dir * 10000);
                        PopAndPushStep(STEP.WaitMoveOk);
                        ShowInfo();
                        break;
                    case STEP.WaitMoveOk:
                        if (motion.IsNormalStop(4))
                        {
                            if (i++ > 4)
                            {
                                Dir *= -1;
                                i = 0;
                            }
                            PopAndPushStep(STEP.Move1000);
                        }
                        ShowInfo();
                        break;
                    default:
                        break;
                }
            }
            return 0;
        }

   
    }
}
