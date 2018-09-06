using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IrixiStepperControllerHelper;
using JPT_TosaTest.Communication;
using JPT_TosaTest.Config.HardwareManager;
using JPT_TosaTest.MotionCards.IrixiCommand;

namespace JPT_TosaTest.MotionCards
{
    public class Motion_IrixiEE0017 : IMotion
    {
        Comport comport = null;
        private IrixiEE0017 _controller=null;

        public MotionCardCfg motionCfg { get; set; }
        public int MAX_AXIS { get; set; }
        public int MIN_AXIS { get; set; }

        public Motion_IrixiEE0017()
        {
           
        }

        public  bool Init(MotionCardCfg motionCfg, ICommunicationPortCfg communicationPortCfg)
        {
            this.motionCfg = motionCfg;
            MAX_AXIS = motionCfg.MaxAxisNo;
            MIN_AXIS = motionCfg.MinAxisNo;
            comport = CommunicationMgr.Instance.FindPortByPortName(motionCfg.PortName) as Comport;
            _controller = IrixiEE0017.CreateInstance(motionCfg.PortName);
            if (comport == null)
                return false;
            _controller = IrixiEE0017.CreateInstance(motionCfg.PortName);
            if (_controller != null)
            {
                _controller.OnAxisPositionChanged += OnAxisPositionChanged;
                if (motionCfg.NeedInit)
                {
                    return _controller.Init(Int32.Parse(comport.ToString().ToLower().Replace("com", "")));
                }
                else
                {
                    return true;
                }
            }
            return false;
        }


        public  bool Deinit()
        {
            try
            {
                comport.ClosePort();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public  bool GetCurrentPos(int AxisNo, out double Pos)
        {
            Pos = 0;
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return _controller.GetCurrentPos(axisIndex, out Pos);
        }

        /// <summary>
        /// 回原点
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <param name="Dir"></param>
        /// <param name="Acc"></param>
        /// <param name="Speed1"></param>
        /// <param name="Speed2"></param>
        /// <returns></returns>
        public  bool Home(int AxisNo, int Dir, double Acc, double Speed1, double Speed2)    //
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return _controller.Home(axisIndex, Dir, Acc, Speed1, Speed2);

        }


        public  bool IsHomeStop(int AxisNo)
        {

            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return _controller.IsHomeStop(axisIndex);

        }

        public  bool IsNormalStop(int AxisNo)
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return _controller.IsNormalStop(axisIndex);

        }

        /// <summary>
        /// 绝对运动
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴号</param>
        /// <param name="Acc">绝对运动加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Pos">绝对位置</param>
        /// <returns></returns>
        public  bool MoveAbs(int AxisNo, double Acc, double Speed, double Pos)
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return _controller.MoveAbs(axisIndex, Acc, Speed, Pos);

        }

        /// <summary>
        /// 绝对运动,Trigger
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴号</param>
        /// <param name="Acc">绝对运动加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Pos">绝对位置</param>
        /// <returns></returns>
        public bool MoveAbs(int AxisNo, double Acc, double Speed, double Pos, EnumTriggerType TriggerType, UInt16 Interval)
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return _controller.MoveAbs(axisIndex, Acc, Speed, Pos, TriggerType, Interval);

        }

        /// <summary>
        /// 相对运动
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴</param>
        /// <param name="Acc">加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Distance">相对距离</param>
        /// <returns></returns>
        public  bool MoveRel(int AxisNo, double Acc, double Speed, double Distance)
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return _controller.MoveRel(axisIndex, Acc, Speed, Distance);

        }

        /// <summary>
        /// 相对运动 Trigger
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴</param>
        /// <param name="Acc">加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Distance">相对距离</param>
        /// <returns></returns>
        public bool MoveRel(int AxisNo, double Acc, double Speed, double Distance, EnumTriggerType TriggerType, UInt16 Interval)
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return _controller.MoveRel(axisIndex, Acc, Speed, Distance, TriggerType, Interval);
        }
       
        /// <summary>
        /// 配置Trigger
        /// </summary>
        /// <param name="ChannelFlags"></param>
        /// <returns></returns>
        public bool SetTrigConfig(byte ChannelFlags)
        {
            return _controller.SetTrigConfig(ChannelFlags);
        }

        public bool GetMemLength(out UInt32 Length)
        {
            Length = 0;
            return _controller.GetMemLength(out Length);
        }
        public bool ReadMem(UInt32 Offset, UInt32 Length, out List<Int16> RawDataList)
        {
            RawDataList = null;
            return _controller.ReadMem(Offset, Length, out RawDataList);
        }

        public bool ClearMem()
        {
            return _controller.ClearMem();
        }
        public bool ReadAD(byte ChannelFlags)
        {
            return _controller.ReadAD(ChannelFlags);
        }
        public  bool SetCurrentPos(int AxisNo, double Pos)
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return false;
        }

        public  bool Stop()
        {
            return _controller.Stop();
        }

        public bool IsAxisInRange(int AxisNo)
        {
            return AxisNo >= MIN_AXIS && AxisNo <= MAX_AXIS;
        }

        private void OnAxisPositionChanged(object sender, Tuple<byte, AxisArgs> e)
        {
            Console.WriteLine(e.Item2.CurAbsPos);
        }
    }
}
