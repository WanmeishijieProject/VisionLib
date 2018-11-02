
using JPT_TosaTest.Config.SoftwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow
{
    public enum STEP : int
    {
        Init,
        CmdGetProduct,
        CmdFindTopLine,
        CmdFindDownLine,
        DO_NOTHING,
        EXIT,
    }
    public class WorkService : WorkFlowBase
    {
        #region
        //start from 0
        private const int CONST_X = 3, CONST_Y1 = 1, CONST_Y2 = 2, CONST_Z = 0, CONST_CY = 5, CONST_CZ = 4, CONST_R = 6;  
        private MotionCards.IMotion  MotionCard=null;
        private IOCards.IIO IOCard = null;
        private int ProductIndex = 0;

        #endregion

        protected override bool UserInit()
        {
            MotionCard = MotionCards.MotionMgr.Instance.FindMotionCardByCardName("Motion_IrixiEE0017[0]");
            IOCard = IOCards.IOCardMgr.Instance.FindIOCardByCardName("IO_IrixiEE0017[0]");
            return MotionCard!=null  && IOCard!=null;
        }
        public WorkService(WorkFlowConfig cfg) : base(cfg)
        {

        }
        protected override int WorkFlow()
        {
            ClearAllStep();
            PushStep(STEP.Init);
            while (!cts.IsCancellationRequested)
            {
                Thread.Sleep(10);
                Step = PeekStep();
                if (bPause || Step==null)
                    continue;             
             
                switch (Step)
                {
                    case STEP.Init:     //初始化
                        ShowInfo();
                        HomeAll();
                        ClearAllStep();
                        break;
                    case STEP.CmdGetProduct:    //取产品
                        GetProduct(++ProductIndex);
                        if (ProductIndex >= 6)
                            ProductIndex = 0;
                        ClearAllStep();
                        break;
                    case STEP.CmdFindTopLine:   //找上表面的线

                        ClearAllStep();
                        break;
                    case STEP.CmdFindDownLine:  //找下表面的线

                        ClearAllStep();
                        break;

                    case STEP.EXIT:
                        return 0;
                }
            }
            return 0;
        }



        #region Private
        private void HomeAll()
        {
            int nStep = 0;
            while (true)
            {
                switch (nStep)
                {
                    case 0:
                        MotionCard.Home(CONST_CZ, 0, 500, 5, 10);
                        MotionCard.Home(CONST_Z, 0, 500, 20, 50);
                        nStep = 1;
                        break;
                    case 1:
                        if (MotionCard.IsHomeStop(CONST_CZ) && MotionCard.IsHomeStop(CONST_Z))
                        {
                            nStep = 2;
                        }
                        break;
                    case 2:
                        MotionCard.Home(CONST_X, 0, 500, 20, 50);
                        MotionCard.Home(CONST_Y2, 0, 500, 20, 50);
                        MotionCard.Home(CONST_Y1, 0, 500, 20, 50);
                        MotionCard.Home(CONST_CY, 0, 500, 5, 10);
                        nStep = 3;
                        break;
                    case 3:
                        if (MotionCard.IsHomeStop(CONST_X) && MotionCard.IsHomeStop(CONST_Y1) && MotionCard.IsHomeStop(CONST_Y2) && MotionCard.IsHomeStop(CONST_CY))
                        {
                            nStep = 4;
                        }
                        break;
                    case 4:
                        
                        return;
                    default:
                        return;

                }
            }
        }
        private void GetProduct(int Index)
        {
            int X = 0, Y1 = 1, Y2 = 2, Z = 3, R = 4, CY = 5, CZ = 6;
            var PtLeftTop=WorkFlowMgr.Instance.GetPoint("左上吸取点");
            var PtRightDown = WorkFlowMgr.Instance.GetPoint("右下吸取点");
            var PtDropDown = WorkFlowMgr.Instance.GetPoint("放置点");

            if (Index < 1 || Index > 6)
                return;

            //2行3列
            double DeltaX = (PtRightDown[X] - PtLeftTop[X]) / 2;

            double TargetX = 0;
            double TargetY1 = 0;
            double TargetZ = PtLeftTop[Z];
            if (Index >= 1 && Index <= 3)
            {
                TargetX= PtLeftTop[X] + DeltaX * (Index - 1);
                TargetY1 =  PtLeftTop[Y1] ;
            }
            else
            {
                TargetX = PtLeftTop[X] + DeltaX * (Index - 4);
                TargetY1 = PtRightDown[Y1];
            }
            

            int nStep = 0;
            while (true)
            {
                switch (nStep)
                {
                    case 0: //移动X，Y1到取料点
                        MotionCard.MoveAbs(CONST_X, 500, 100, TargetX);
                       
                        nStep = 1;
                        break;
                    case 1:
                        if (MotionCard.IsNormalStop(CONST_X))
                        {
                            MotionCard.MoveAbs(CONST_Y1, 500, 100, TargetY1);
                            nStep = 2;
                        }
                        break;
                    case 2:
                        if (MotionCard.IsNormalStop(CONST_Y1))
                        {
                            nStep = 3;
                        }
                        break;
                    case 3: //下降Z轴
                        MotionCard.MoveAbs(CONST_Z, 500, 100, TargetZ);
                        nStep = 4;
                        break;
                    case 4:
                        if (MotionCard.IsNormalStop(CONST_Z))
                        {
                            nStep = 5;
                        }
                        break;
                    case 5: //吸真空
                        nStep = 6;
                        break;    
                     case 6:
                        MotionCard.MoveAbs(CONST_Z,500,100,0);
                        nStep = 7;
                        break;
                    case 7: //Z轴抬起
                        if (MotionCard.IsNormalStop(CONST_Z))
                        {
                            MotionCard.MoveAbs(CONST_X,500,100, PtDropDown[X]);
                            nStep = 8;
                        }
                        break;
                    case 8: //移动到放置点的X位置并下降Z
                        if (MotionCard.IsNormalStop(CONST_X))
                        {
                            MotionCard.MoveAbs(CONST_Z, 500, 100, PtDropDown[Z]);
                            nStep = 9;
                        }
                        break;
                    case 9: //移动Y2过去
                        if (MotionCard.IsNormalStop(CONST_Z))
                        {
                            MotionCard.MoveAbs(CONST_Y2, 500, 100, PtDropDown[Y2]);
                            nStep = 10;
                        }
                        break;
                    case 10: //
                        if (MotionCard.IsNormalStop(CONST_Y2))
                        {
                            nStep = 11;
                        }
                        break;
                    case 11:    //等待调整位置
                        return;
                    default:
                        return;
                }
            }

        }
        private void BackToInitPos()
        {
        }
        #endregion

    }
}
