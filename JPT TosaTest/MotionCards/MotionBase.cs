using JPT_TosaTest.Config.HardwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards
{
    public abstract class MotionBase
    {
        protected int MAX_AXIS;
        protected int MIN_AXIS;
        public MotionCardCfg motionCfg=null;
        /// <summary>
        /// 板卡初始化
        /// </summary>
        /// <returns></returns>
        public abstract bool Init(MotionCardCfg motionCfg, ICommunicationPortCfg communicationPort);

        /// <summary>
        /// 释放板卡资源
        /// </summary>
        /// <returns></returns>
        public abstract bool Deinit();

        /// <summary>
        /// 绝对位置移动
        /// </summary>
        /// <param name="AxisNo">轴号</param>
        /// <param name="Acc">加速度 mm/s*s</param>
        /// <param name="Speed">速度mm/s</param>
        /// <param name="Pos">绝对位置</param>
        /// <returns></returns>
        public abstract bool MoveAbs(int AxisNo, double Acc, double Speed, double Pos);

        /// <summary>
        /// 相对移动
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <param name="Acc">加速度 mm/s*s</param>
        /// <param name="Speed">速度 mm/s</param>
        /// <param name="Distance">相对移动距离，正负号表示方向</param>
        /// <returns></returns>
        public abstract bool MoveRel(int AxisNo, double Acc, double Speed, double Distance);

        /// <summary>
        /// 回原点
        /// </summary>
        /// <param name="AxisNo">轴号，从零开始</param>
        /// <param name="Dir">回零方向0-负方向，1-正方向</param>
        /// <param name="Acc"></param>
        /// <param name="Speed1"></param>
        /// <param name="Speed2"></param>
        /// <returns></returns>
        public abstract bool Home(int AxisNo, int Dir, double Acc, double Speed1, double Speed2);

        /// <summary>
        /// 是否回原点到位
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <returns></returns>
        public abstract bool IsHomeStop(int AxisNo);

        /// <summary>
        /// 是否正常停止
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <returns></returns>
        public abstract bool IsNormalStop(int AxisNo);

        /// <summary>
        /// 获取当前位置
        /// </summary>
        /// <param name="AxisNo">轴号，从0开始</param>
        /// <param name="Pos">输出当前位置值，单位时mm</param>
        /// <returns></returns>
        public abstract bool GetCurrentPos(int AxisNo, out double Pos);

        /// <summary>
        /// 设置当前位置
        /// </summary>
        /// <param name="AxisNo">轴号</param>
        /// <param name="Pos">位置值，单位是mm</param>
        /// <returns></returns>
        public abstract bool SetCurrentPos(int AxisNo, double Pos);

        public abstract bool Stop();

        public  bool IsAxisInRange(int AxisNo)
        {
            return AxisNo >= MIN_AXIS && AxisNo <= MAX_AXIS;
        }
    }
}
