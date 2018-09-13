
using JPT_TosaTest.Config.SoftwareManager;
using JPT_TosaTest.IOCards;
using JPT_TosaTest.MotionCards;
using JPT_TosaTest.MotionCards.IrixiCommand;
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
            HomeX,
            HomeY,
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
            motion = MotionMgr.Instance.FindMotionCardByAxisIndex(4) as Motion_IrixiEE0017;
            io = IOCardMgr.Instance.FindIOCardByCardName("IO_IrixiEE0017[0]") as IO_IrixiEE0017;

            motion.Home(2,0,0,0,0);
            motion.Home(3,0,0, 0, 0);
            //motion.MoveRel(2, 0, 1000, 10000);
            //motion.MoveAbs(3, 0, 1000, 10000);
           // motion.MoveAbs(3, 0, 10000, 10000);
            //motion.SetTrigConfig(0xFF);
            //motion.ClearMem();
            //motion.MoveRel(4, 0, 5000, 1000, EnumTriggerType.ADC, 100);
            //motion.ClearMem();

            //motion.GetMemLength(out UInt32 Len);
            //motion.ReadMem(0, Len, out List<Int16> RawData);
            //motion.GetCurrentPos(4, out double pos);
            /*io.WriteIoOutBit(1, false);
            io.WriteIoOutBit(2, false);
            io.WriteIoOutBit(4, false);
            io.WriteIoOutBit(3, false);

            io.WriteIoOutBit(1, true);
            io.WriteIoOutBit(2, true);
            io.WriteIoOutBit(4, true);
            io.WriteIoOutBit(3, true);*/
            //io.ReadIoInWord(0, out int inValue);
            //io.WriteIoOutBit(0, false);
            //io.WriteIoOutBit(1, false);
            io.ReadIoInWord(0, out int value);
            io.ReadIoOutWord(0, out value);
            //motion.DoBlindSearch(3, 4, 500, 10, 5000, 2);
            //motion.Stop();

            //bool bRet=io.ReadIoOutWord(0, out int value1);
            //io.ReadIoInWord(0, out value1);
            //if(!bRet)
            //    Console.WriteLine("----------------------Failed------------------------");

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
                        PopAndPushStep(STEP.HomeX);
                        ShowInfo();
                        break;
                    case STEP.HomeX:
                        if (motion.Home(2,0,0,0,0))
                        {
                            PopAndPushStep(STEP.HomeY);
                        }
                        ShowInfo();
                        break;
                    case STEP.HomeY:
                        if(motion.Home(3, 0, 0, 0, 0))
                            PopAndPushStep(STEP.WaitHomeOk);
                        break;
                    case STEP.WaitHomeOk:
                        if (motion.IsHomeStop(2) && motion.IsHomeStop(3))
                        {
                            PopAndPushStep(STEP.MoveX_To_Aligment);
                            //return 0;
                        }
                        break;
                    case STEP.MoveX_To_Aligment:
                        if (motion.MoveAbs(2,0,10000,10000))
                        {
                            PopAndPushStep(STEP.Wait_MoveX_To_Aligment);
                        }
                        break;
               
                    case STEP.Wait_MoveX_To_Aligment:
                        if (motion.IsNormalStop(2))
                        {
                            PopAndPushStep(STEP.MoveY_To_Aligment);
                        }
                        ShowInfo();
                        break;
                    case STEP.MoveY_To_Aligment:
                        if(motion.MoveAbs(3, 0, 10000, 10000))
                            PopAndPushStep(STEP.Wait_MoveY_To_Aligment);
                        ShowInfo();
                        break;
                    case STEP.Wait_MoveY_To_Aligment:
                        if (motion.IsNormalStop(3))
                        {
                            //Thread.Sleep(1000);
                            PopAndPushStep(STEP.StartAlignment);
                        }
                        break;
                    case STEP.StartAlignment:
                        if (motion.DoBlindSearch(2,3,1000,10,10000,5))
                            PopAndPushStep(STEP.DO_NOTHING);
                        break;
                    case STEP.DO_NOTHING:
                        Thread.Sleep(200);
                        PopAndPushStep(STEP.EXIT);
                        ShowInfo();
                        break;
                    case STEP.EXIT:
                        return 0;
                    default:
                        break;
                }
            }
            return 0;
        }

   
    }
}
