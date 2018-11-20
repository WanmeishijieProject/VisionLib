using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPT_TosaTest.Communication;
using JPT_TosaTest.Config.HardwareManager;
using JPT_TosaTest.MotionCards.IrixiCommand;
using AxisParaLib;



namespace JPT_TosaTest.MotionCards
{
    public enum EnumIrixiMotionError
    {
        ERR_NONE,
        ERR_PARA,
        ERR_NOT_INIT,
        ERR_NOT_HOMED,
        ERR_BUSY,
        ERR_CWLS,
        ERR_CCWLS,
        ERR_EMERGENCY,
        ERR_USER_STOPPED,
        ERR_NO_STAGE_DETECTED,
        ERR_UNKNOWN,
        ERR_SYS_MCSU_ID = 0x80,
        ERR_SYS_CANBUS_RX,
        ERR_SYS_PARAM,
        ERR_SYS_CSS1_Triggered,
        ERR_SYS_CSS2_Triggered,
        ERR_SYS_BLINDSEARCH,

        // NOTE the following errors are only defined in PC side!
        ERR_TIMEOUT,
        ERR_OPERATION_CANCELLED,
    }

    public class Motion_IrixiEE0017 : IMotion
    {
        Comport comport = null;
        private IrixiEE0017 _controller=null;
      

        public event AxisStateChange OnAxisStateChanged;
        public event ErrorOccur OnErrorOccured;

        public MotionCardCfg motionCfg { get; set; }
        public int MAX_AXIS { get; set; }
        public int MIN_AXIS { get; set; }
        public AxisArgs[] AxisArgsList { get; }

        public Motion_IrixiEE0017()
        {
            AxisArgsList = new AxisArgs[12];
            for (int i = 0; i < 12; i++)
                AxisArgsList[i] = new AxisArgs();
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
                _controller.OnAxisStateChanged += OnIrixiAxisStateChanged;
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
                throw;
            }
        }

        public  bool GetCurrentPos(int AxisNo, out double Pos)
        {
            Pos = 0;
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }
            Pos = AxisArgsList[AxisNo].CurAbsPos;
            return true;
        }

        public bool GetAxisState(int AxisNo, out AxisArgs state)
        {
            state = null;
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }
            state = AxisArgsList[AxisNo];
            return true;
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
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }
            int axisIndex = AxisNo + MIN_AXIS;
            return _controller.Home(axisIndex, Dir, Acc, Speed1, Speed2);

        }

        public  bool IsHomeStop(int AxisNo)
        {
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }
            int axisIndex = AxisNo + MIN_AXIS;
            return _controller.IsHomeStop(axisIndex);

        }

        public  bool IsNormalStop(int AxisNo)
        {
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }
            int axisIndex = AxisNo + MIN_AXIS;
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
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }
            int axisIndex = AxisNo + MIN_AXIS;
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
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }
            int axisIndex = AxisNo + MIN_AXIS;
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
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }  
            int axisIndex = AxisNo + MIN_AXIS;
            if (!IsPosValid(AxisNo, Distance))
                throw new Exception($"Axis{AxisNo} can't reach the specified location");
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
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }
            int axisIndex = AxisNo + MIN_AXIS;
            if (!IsPosValid(AxisNo, Distance))
                throw new Exception($"Axis{AxisNo} can't reach the specified location");
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

        public bool ReadAD(EnumADCChannelFlags ChannelFlags, out List<UInt16> values)
        {
            return _controller.ReadAD(ChannelFlags, out values);
        }

        public bool SetCssEnable(EnumCssChannel CssChannel, bool IsEnable)
        {
            return _controller.EnableCss(CssChannel, IsEnable);
        }

        public bool SetCssThreshold(EnumCssChannel CssChannel, UInt16 Low, UInt16 Hight)
        {
            return _controller.SetCssThreshold(CssChannel, Low, Hight);
        }


        public  bool Stop()
        {
            return _controller.Stop();
        }

        public bool IsAxisInRange(int AxisNo)   //给Mgr用来查询板卡使用的，别的函数都是从0开始
        {
            return AxisNo >= MIN_AXIS && AxisNo <= MAX_AXIS;
        }

        public bool DoBlindSearch(int XAxisNo, int YAxisNo, double Range, double Gap, double Speed, double Interval)
        {
            if (XAxisNo > MAX_AXIS- MIN_AXIS || XAxisNo < 0 || YAxisNo>MAX_AXIS-MIN_AXIS || YAxisNo<0)
            {
                return false;
            }
            int XaxisIndex = XAxisNo  + MIN_AXIS;
            int YaxisIndex = YAxisNo  + MIN_AXIS;
            return _controller.DoBindSearch(XaxisIndex, YaxisIndex, Range, Gap, Speed, Interval);
        }

        
        public bool SetCurrentPos(int AxisNo, double Pos)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reset Mcsu error,All axises
        /// </summary>
        /// <returns></returns>
        public bool Reset()
        {
            return true;
        }



        public void SetAxisPara(int AxisNo, AxisSetting Setting)
        {
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return;
            }
            if (Setting == null)
                return;

            AxisArgsList[AxisNo].AxisName = Setting.AxisName;
            AxisArgsList[AxisNo].AxisNo = Setting.AxisNo;
            AxisArgsList[AxisNo].LimitN = Setting.LimitN;
            AxisArgsList[AxisNo].LimitP = Setting.LimitP;
            AxisArgsList[AxisNo].HomeOffset = Setting.HomeOffset;
            AxisArgsList[AxisNo].HomeMode = (int)Setting.HomeMode;
            AxisArgsList[AxisNo].AxisType = Setting.AxisType;
            AxisArgsList[AxisNo].BackwardCaption = Setting.BackwardCaption;
            AxisArgsList[AxisNo].ForwardCaption = Setting.ForwardCaption;
            AxisArgsList[AxisNo].MaxSpeed = Setting.MaxSpeed;

            _controller.SetAxisPara(AxisNo + MIN_AXIS, Setting.GainFactor, Setting.LimitP, Setting.LimitN, Setting.HomeOffset, (int)Setting.HomeMode, Setting.AxisName);
        }


        public bool SetMode(int AxisNo, int mode)
        {
            if (AxisNo > MAX_AXIS - MIN_AXIS || AxisNo < 0)
            {
                return false;
            }
            return _controller.SetMode(AxisNo + MIN_AXIS,(byte)mode);
        }

        private bool IsPosValid(int AxisNo, double TargetPosRelative)
        {
            if (GetCurrentPos(AxisNo, out double CurPos))
            {
                if (TargetPosRelative + CurPos > AxisArgsList[AxisNo].LimitP || TargetPosRelative + CurPos < AxisArgsList[AxisNo].LimitN)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private string ParseErrorCode(int error)
        {
            if(Enum.IsDefined(typeof(EnumIrixiMotionError), error))
                return ((EnumIrixiMotionError)error).ToString();
            return "未定义错误类型";
        }

        private void OnIrixiAxisStateChanged(object sender, Tuple<byte, AxisArgs> e)
        {
            int AxisNo = e.Item1 - 1;
            AxisArgsList[AxisNo].CurAbsPos = e.Item2.CurAbsPos;
            AxisArgsList[AxisNo].IsBusy = e.Item2.IsBusy;
            AxisArgsList[AxisNo].IsHomed = e.Item2.IsHomed;
            AxisArgsList[AxisNo].IsInRequest = e.Item2.IsInRequest;
            OnAxisStateChanged?.Invoke(this, e.Item1 - 1, AxisArgsList[AxisNo]);
            if (AxisArgsList[AxisNo].ErrorCode != e.Item2.ErrorCode)
            {
                AxisArgsList[AxisNo].ErrorCode = e.Item2.ErrorCode;
                if (e.Item2.ErrorCode != 0)
                {
                    OnErrorOccured?.Invoke(this, e.Item2.ErrorCode, ParseErrorCode(e.Item2.ErrorCode));
                }
            }
        }

    }


}
