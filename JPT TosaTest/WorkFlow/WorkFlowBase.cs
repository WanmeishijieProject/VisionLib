using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using JPT_TosaTest.Config.SoftwareManager;
using JPT_TosaTest.Config.SystemCfgManager;

namespace JPT_TosaTest.WorkFlow
{
    public delegate void StationInfoHandler(int Index, string StationName, string Msg);
   
    public class WorkFlowBase
    {
        public bool Enable;
        public string StationName;
        public int StationIndex;
        protected bool bPause = false;
        public event StationInfoHandler OnStationInfoChanged;
        protected WorkFlowConfig cfg = null;
        protected CancellationTokenSource cts =new CancellationTokenSource();
        protected Stack<object> nStepStack=new Stack<object>();
        protected Task t = null; 
        protected object nStep;
        protected object PeekStep() { return nStepStack.Peek(); }
        protected void PushStep(object nStep) { nStepStack.Push(nStep); }
        protected void PopAndPushStep(object nStep) { nStepStack.Pop(); nStepStack.Push(nStep); }
        protected void PushBatchStep(object[] nSteps)
        {
            foreach (var step in nSteps)
                nStepStack.Push(step);
        }
        protected void PopStep() { nStepStack.Pop(); }
        protected void ClearAllStep() { nStepStack.Clear(); }
        protected int GetCurStepCount() { return nStepStack.Count; }
        protected virtual bool UserInit() { return true; }
        public WorkFlowBase(WorkFlowConfig cfg) { this.cfg = cfg; }
        public void ShowInfo(string strInfo=null)    //int msg, int iPara, object lParam
        {
            if (strInfo == null || strInfo.Trim().ToString() == "")
                strInfo = nStep.ToString();
            DateTime dt = DateTime.Now;
            OnStationInfoChanged(StationIndex, cfg.Name, string.Format("{0:D2}:{1:D2}:{2:D2}  {3:D2}", dt.Hour, dt.Minute, dt.Second, strInfo));
        }
        public bool Start()
        {
            bPause = false;
            if (!UserInit())
            {
                return false;
            }
            else if (t==null || t.Status == TaskStatus.Canceled || t.Status == TaskStatus.RanToCompletion)
            {
                cts = new CancellationTokenSource();
                t = new Task(() => ThreadFunc(this), cts.Token);
                t.Start();
            }
            return true;
        }
        public bool Stop()
        {
            cts.Cancel();
            return true;
        }
        public bool Pause()
        {
            this.bPause = true;
            return true;
        }
        private static int ThreadFunc(object o) { return (o as WorkFlowBase).WorkFlow(); }
        protected virtual int WorkFlow() { return 0; }
        public void WaitComplete()
        {
            //if (t != null)
            //    t.Wait(5000);
        }
        protected SystemParaModel SysPara = null;
    }
}
