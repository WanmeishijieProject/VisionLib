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

        }
        private static readonly Lazy<MotionMgr> _instance = new Lazy<MotionMgr>(() => new MotionMgr());
        public static MotionMgr Instance
        {
            get { return _instance.Value; }
        }

        private Dictionary<string, IMotion> MotionDic = new Dictionary<string, IMotion>();

        public void AddMotionCard(string CardName, IMotion MotionCard)
        {
            bool bFind = false;
            foreach (var it in MotionDic)
            {
                if (it.Key == CardName)
                {
                    bFind = true;
                    return;
                }
            }
            if (!bFind)
                MotionDic.Add(CardName, MotionCard);
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
                if (it.Value.IsAxisInRange(AxisNo))
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
                return MotionCard.MoveAbs(AxisNo, Acc, Speed, Pos);
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
                return MotionCard.MoveRel(AxisNo, Acc, Speed, Distance);
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
                return MotionCard.Home(AxisNo, Dir, Acc, Speed1, Speed2);
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
                return MotionCard.IsHomeStop(AxisNo);
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
                return MotionCard.IsNormalStop(AxisNo);
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
                return MotionCard.GetCurrentPos(AxisNo,out Pos);
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
                return MotionCard.SetCurrentPos(AxisNo, Pos);
            }
            return false;
        }
    }
}
