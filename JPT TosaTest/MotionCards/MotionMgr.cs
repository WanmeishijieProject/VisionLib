using AxisParaLib;
using JPT_TosaTest.IOCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards
{ 
    public class MotionMgr
    {
        private MotionMgr()
        {
            MotionDic = new Dictionary<string, IMotion>();
        }
        private static readonly Lazy<MotionMgr> _instance = new Lazy<MotionMgr>(() => new MotionMgr());
        public static MotionMgr Instance
        {
            get { return _instance.Value; }
        }

        public Dictionary<string, IMotion> MotionDic { get; }

        public void AddMotionCard(string CardName, IMotion MotionCard)
        {
            if (!MotionDic.Keys.Contains(CardName))
            {
                MotionDic.Add(CardName, MotionCard);
            }
        }

        private void MotionCard_OnErrorOccured(int ErrorCode, string ErrorMsg)
        {
            throw new NotImplementedException();
        }

        private void MotionCard_OnAxisStateChanged(int AxisNo, AxisArgs axisState)
        {
            throw new NotImplementedException();
        }

        public void RemoveMotionCard(string CardName)
        {
            foreach (var it in MotionDic)
            {
                if (it.Key == CardName)
                    MotionDic.Remove(CardName);
            }
        }


        public IMotion FindMotionCardByAxisIndex(int AxisNo)
        {
            foreach (var it in MotionDic)
            {
                if (it.Value.IsAxisInRange(AxisNo-it.Value.MIN_AXIS))
                    return it.Value;
            }
            return null;         
        }
        public IMotion FindMotionCardByCardName(string CardName)
        {
            foreach (var it in MotionDic)
            {
                if (it.Key.Equals(CardName))
                    return it.Value;
            }
            return null;
        }
        public IMotion FindMotionCardByCardNo(int CardIndex)
        {
            int i = 0;
            foreach (var it in MotionDic)
            {
                if (i++ == CardIndex)
                    return it.Value;
            }
            return null;
        }


        /// <summary>
        /// 释放板卡资源,初始化在添加板卡的时候完成
        /// </summary>
        /// <returns></returns>
        public bool Deinit(string CardName)
        {
            var card = FindMotionCardByCardName(CardName);
            if (card != null)
                return card.Deinit();
            return false;
        }
        public bool Deinit(int AxisNo)
        {
            var card = FindMotionCardByAxisIndex(AxisNo);
            if (card != null)
                return card.Deinit();
            return false;
        }
        /// <summary>
        /// 绝对位置移动
        /// </summary>
        /// <param name="AxisNo">轴号</param>
        /// <param name="Acc">加速度 mm/s*s</param>
        /// <param name="Speed">速度mm/s</param>
        /// <param name="Pos">绝对位置</param>
        /// <returns></returns>
        public bool MoveAbs(int AxisNo, double Acc, double Speed, double Pos)
        {
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);
            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.MoveAbs(AxisIndex, Acc, Speed, Pos);
            }
            return false;
        }

        /// <summary>
        /// 相对移动
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <param name="Acc">加速度 mm/s*s</param>
        /// <param name="Speed">速度 mm/s</param>
        /// <param name="Distance">相对移动距离，正负号表示方向</param>
        /// <returns></returns>
        public bool MoveRel(int AxisNo, double Acc, double Speed, double Distance)
        {
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);
            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.MoveRel(AxisIndex, Acc, Speed, Distance);
            }
            return false;
        }

        /// <summary>
        /// 回原点
        /// </summary>
        /// <param name="AxisNo">轴号，从零开始</param>
        /// <param name="Dir">回零方向0-负方向，1-正方向</param>
        /// <param name="Acc">回零加速度</param>
        /// <param name="Speed1">搜索原点1速度</param>
        /// <param name="Speed2">爬行速度</param>
        /// <returns></returns>
        public bool Home(int AxisNo, int Dir, double Acc, double Speed1, double Speed2)
        {
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);

            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.Home(AxisIndex, Dir, Acc, Speed1, Speed2);
            }
            return false;
        }

        /// <summary>
        /// 是否回原点到位
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <returns></returns>
        public bool IsHomeStop(int AxisNo)
        {
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);
            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.IsHomeStop(AxisIndex);
            }
            return false;
        }

        /// <summary>
        /// 是否正常停止
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <returns></returns>
        public bool IsNormalStop(int AxisNo)
        {
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);
            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.IsNormalStop(AxisIndex);
            }
            return false;
        }

        /// <summary>
        /// 获取当前位置
        /// </summary>
        /// <param name="AxisNo">轴号，从0开始</param>
        /// <param name="Pos">输出当前位置值，单位时mm</param>
        /// <returns></returns>
        public bool GetCurrentPos(int AxisNo, out double Pos)
        {
            Pos = 0.0;
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);
            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.GetCurrentPos(AxisIndex,out Pos);
            }
            return false;
        }

        public bool GetAxisState(int AxisNo, out AxisArgs axisArgs)
        {
            axisArgs = null;
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);
            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.GetAxisState(AxisIndex, out axisArgs);
            }
            return false;
        }
        /// <summary>
        /// 设置当前位置
        /// </summary>
        /// <param name="AxisNo">轴号</param>
        /// <param name="Pos">位置值，单位是mm</param>
        /// <returns></returns>
        public bool SetCurrentPos(int AxisNo, double Pos)
        {
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);
            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.SetCurrentPos(AxisIndex, Pos);
            }
            return false;
        }

        public bool Reset(int AxisNo)
        {
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);
            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.Reset();
            }
            return false;
        }

        public bool Stop(int AxisNo)
        {
            var MotionCard = FindMotionCardByAxisIndex(AxisNo);
            if (MotionCard != null)
            {
                int AxisIndex = AxisNo - MotionCard.MIN_AXIS;
                return MotionCard.Stop();
            }
            return false;
        }

        public bool StopAll()
        {
            foreach (var motion in MotionDic)
            {
                motion.Value.Stop();
            }
            return true;
        }
    }
}
