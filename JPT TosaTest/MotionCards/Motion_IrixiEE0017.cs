using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPT_TosaTest.Communication;
using JPT_TosaTest.Config.HardwareManager;
using AxisParaLib;
using M12;
using M12.Definitions;
using M12.Base;
using M12.Commands.Alignment;

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

    public enum EnumTriggerType
    {
        TrigAdc,
        TrigOut
    }

    public class Motion_IrixiEE0017 : IMotion
    {
        Comport comport = null;
        //private IrixiEE0017 _controller=null;
        private M12Wrapper _controller = null;

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

        public bool Init(MotionCardCfg motionCfg, ICommunicationPortCfg communicationPortCfg)
        {
            try
            {
                this.motionCfg = motionCfg;
                MAX_AXIS = motionCfg.MaxAxisNo;
                MIN_AXIS = motionCfg.MinAxisNo;
                ComportCfg portCfg = communicationPortCfg as ComportCfg;
                comport = CommunicationMgr.Instance.FindPortByPortName(motionCfg.PortName) as Comport;
                _controller = M12Wrapper.CreateInstance(portCfg.Port, portCfg.BaudRate);
                _controller.OnUnitStateUpdated += OnIrixiAxisStateChanged;
                _controller.Open();

                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool Deinit()
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

        public bool GetCurrentPos(int AxisNo, out double Pos)
        {
            Pos = 0;
            if (!IsAxisInRange(AxisNo))
            {
                return false;
            }
            Pos = AxisArgsList[AxisNo].CurAbsPos;
            return true;
        }

        public bool GetAxisState(int AxisNo, out AxisArgs state)
        {
            var s = _controller.GetUnitState((UnitID)(AxisNo + (int)UnitID.U1));
            OnIrixiAxisStateChanged(this, s);
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
        public bool Home(int AxisNo, int Dir, double Acc, double Speed1, double Speed2)    //
        {
            HomeAsync(AxisNo, Dir, Acc, Speed1, Speed2);
            return true;
        }
        private async Task<bool> HomeAsync(int AxisNo, int Dir, double Acc, double Speed1, double Speed2)
        {
            return await Task.Run(() =>
            {
                if (!IsAxisInRange(AxisNo))
                {
                    return false;
                }
                int axisIndex = AxisNo + 1;
                if (Enum.IsDefined(typeof(M12.Definitions.UnitID), axisIndex))
                {
                    try
                    {
                        _controller.Home((M12.Definitions.UnitID)axisIndex, (ushort)Acc, (byte)Speed1, (byte)Speed2);
                    }
                    catch
                    {
                        return false;
                    }
                    return true;
                }
                {
                    return false;
                }
            });
        }


        public bool IsHomeStop(int AxisNo)
        {
            if (!IsAxisInRange(AxisNo))
            {
                return false;
            }
            int axisIndex = AxisNo + 1;
            if (Enum.IsDefined(typeof(M12.Definitions.UnitID), axisIndex))
            {
                var state = _controller.GetUnitState((M12.Definitions.UnitID)axisIndex);
                return state.IsHomed;
            }
            return false;

        }

        public bool IsNormalStop(int AxisNo)
        {
            if (!IsAxisInRange(AxisNo))
            {
                return false;
            }
            int axisIndex = AxisNo + 1;
            Thread.Sleep(100);
            if (Enum.IsDefined(typeof(M12.Definitions.UnitID), axisIndex))
            {
                var state = _controller.GetUnitState((M12.Definitions.UnitID)axisIndex);
                return !state.IsBusy;
            }
            return false;
        }

        /// <summary>
        /// 绝对运动
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴号</param>
        /// <param name="Acc">绝对运动加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Pos">绝对位置</param>
        /// <returns></returns>
        public bool MoveAbs(int AxisNo, double Acc, double Speed, double Pos)
        {
            MoveAbsAsync(AxisNo, Acc, Speed, Pos);
            return true;
        }

        public async Task<bool> MoveAbsAsync(int AxisNo, double Acc, double Speed, double Pos)
        {
            return await Task.Run(() =>
            {
                if (!IsAxisInRange(AxisNo))
                {
                    return false;
                }
                int axisIndex = AxisNo + 1;
                if (Enum.IsDefined(typeof(M12.Definitions.UnitID), axisIndex))
                {
                    var state = _controller.GetUnitState((M12.Definitions.UnitID)axisIndex);
                    int CurPos = state.AbsPosition;
                    int TargetPos = (int)(AxisArgsList[AxisNo].GainFactor * Pos);
                    int StepsInt = TargetPos - CurPos;
                    try
                    {
                        _controller.Move((M12.Definitions.UnitID)(axisIndex), StepsInt, (byte)Speed);
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        /// <summary>
        /// 绝对运动,Trigger
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴号</param>
        /// <param name="Acc">绝对运动加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Pos">绝对位置</param>
        /// <returns></returns>
        public async Task<bool> MoveAbs(int AxisNo, double Acc, double Speed, double Pos, EnumTriggerType TriggerType, UInt16 Interval)
        {
            return await Task.Run(() =>
            {
                if (!IsAxisInRange(AxisNo))
                {
                    return false;
                }
                int axisIndex = AxisNo + 1;
                if (Enum.IsDefined(typeof(M12.Definitions.UnitID), axisIndex))
                {
                    var state = _controller.GetUnitState((M12.Definitions.UnitID)axisIndex);
                    int CurPos = state.AbsPosition;
                    int TargetPos = (int)(AxisArgsList[AxisNo].GainFactor * Pos);
                    int StepsInt = TargetPos - CurPos;
                    UInt16 IntervalPause = (UInt16)(Interval * AxisArgsList[AxisNo].GainFactor);
                    try
                    {
                        _controller.MoveTriggerADC((M12.Definitions.UnitID)(axisIndex), StepsInt, (byte)Speed, IntervalPause);
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        /// <summary>
        /// 相对运动
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴</param>
        /// <param name="Acc">加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Distance">相对距离</param>
        /// <returns></returns>
        public bool MoveRel(int AxisNo, double Acc, double Speed, double Distance)
        {
            MoveRelAsync(AxisNo, Acc, Speed, Distance);
            return true;
        }
        private async Task<bool> MoveRelAsync(int AxisNo, double Acc, double Speed, double Distance)
        {
            return await Task.Run(() =>
            {
                if (!IsAxisInRange(AxisNo))
                {
                    return false;
                }
                int axisIndex = AxisNo + 1;
                if (!IsPosValid(AxisNo, Distance))
                    throw new Exception($"Axis{AxisNo} can't reach the specified location");
                if (Enum.IsDefined(typeof(M12.Definitions.UnitID), axisIndex))
                {
                    int StepsInt = (int)(Distance * AxisArgsList[AxisNo].GainFactor);
                    byte SpeedInt = (byte)Speed;
                    try
                    {
                        _controller.Move((M12.Definitions.UnitID)axisIndex, StepsInt, SpeedInt);
                    }
                    catch
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            });
        }


        /// <summary>
        /// MoveRel With trigger
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <param name="Acc"></param>
        /// <param name="Speed"></param>
        /// <param name="Distance"></param>
        /// <param name="TriggerType">目前只支持一种Trig，就是ADCTrigger</param>
        /// <param name="Interval"></param>
        /// <returns></returns>
        public async Task<bool> MoveRel(int AxisNo, double Acc, double Speed, double Distance, EnumTriggerType TriggerType, double Interval)
        {
            if (!IsAxisInRange(AxisNo))
            {
                return false;
            }
            int axisIndex = AxisNo + 1;
            if (!IsPosValid(AxisNo, Distance))
                throw new Exception($"Axis{AxisNo} can't reach the specified location");
            if (Enum.IsDefined(typeof(M12.Definitions.UnitID), axisIndex))
            {
                int StepsInt = (int)(Distance * AxisArgsList[AxisNo].GainFactor);
                byte SpeedInt = (byte)Speed;
                UInt16 IntervalUInt = (UInt16)(Interval * AxisArgsList[AxisNo].GainFactor);
                try
                {
                    _controller.MoveTriggerADC((M12.Definitions.UnitID)axisIndex, StepsInt, SpeedInt, IntervalUInt);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 配置Trigger
        /// </summary>
        /// <param name="ChannelFlags"></param>
        /// <returns></returns>
        public bool SetTrigConfig(M12.Definitions.ADCChannels ChannelFlags)
        {

            _controller.ConfigADCTrigger(ChannelFlags);
            return true;
        }

        public bool GetMemLength(out UInt32 Length)
        {
            Length = _controller.GetMemoryLength();
            return true;
        }

        public bool ReadMem(UInt32 Offset, UInt32 Length, out List<double> RawDataList)
        {
            RawDataList = _controller.ReadMemory(Offset, Length);
            return true;
        }

        public bool ClearMem()
        {
            _controller.ClearMemory();
            return true;
        }

        public bool ReadAD(ADCChannels ChannelFlags, out double[] values)
        {
            values = _controller.ReadADC(ChannelFlags);
            return true;
        }
        public void SaveUnitENV(UnitID UnitID)
        {
            _controller.SaveUnitENV(UnitID);
        }
        public bool SetCssEnable(CSSCH CssChannel, bool IsEnable)
        {
            _controller.SetCSSEnable(CssChannel, IsEnable);
            return true;
        }

        public bool SetCssThreshold(CSSCH CssChannel, UInt16 Low, UInt16 Hight)
        {

            _controller.SetCSSThreshold(CssChannel, Low, Hight);
            return true;
        }




        public bool Stop(int AxisNo)
        {
            if (Enum.IsDefined(typeof(UnitID), AxisNo + 1))
            {
                UnitID Axis = (UnitID)Enum.Parse(typeof(UnitID), (AxisNo + 1).ToString());
                _controller.Stop(Axis);
            }
            return false;
        }
        public bool StopAll()
        {
            // _controller.Stop(UnitID.ALL); 不起作用
            for (int i = 0; i < MAX_AXIS - MIN_AXIS + 1; i++)
            {
                _controller.Stop((UnitID)(i + (int)UnitID.U1));
            }
            return false;
        }

        public bool IsAxisInRange(int AxisNo)
        {
            return AxisNo >= 0 && AxisNo <= MAX_AXIS - MIN_AXIS;
        }

        public bool DoBlindSearch(UnitID XAxis, UnitID YAxis, double Range, double Gap, double Speed, double Interval, ADCChannels AdcUsed, out List<Point3D> ScanResults)
        {
            ScanResults = new List<Point3D>();
            int XAxisNo = (int)XAxis - 1;
            int YAxisNo = (int)YAxis - 1;
            if (XAxisNo > MAX_AXIS - MIN_AXIS || XAxisNo < 0 || YAxisNo > MAX_AXIS - MIN_AXIS || YAxisNo < 0)
            {
                return false;
            }
            int XaxisIndex = XAxisNo + 1;
            int YaxisIndex = YAxisNo + 1;
            UInt16 RangePause = (UInt16)(AxisArgsList[XAxisNo].GainFactor*Range);
            UInt16 GapPause = (UInt16)(AxisArgsList[XAxisNo].GainFactor * Gap);
            byte SpeedPause = (byte)Speed;
            UInt16 IntervalPause = (UInt16)(AxisArgsList[XAxisNo].GainFactor * Interval);

            var HArgs = new BlindSearchArgs() {
                 Interval= IntervalPause,
                 Speed=SpeedPause,
                 Gap=GapPause,
                 Unit= XAxis,
                 Range=RangePause
            };

            var VArgs = new BlindSearchArgs()
            {
                Interval = IntervalPause,
                Speed = SpeedPause,
                Gap = GapPause,
                Unit = YAxis,
                Range = RangePause
            };

            _controller.StartBlindSearch(HArgs, VArgs, AdcUsed, out ScanResults);
            return true;
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
            if (!IsAxisInRange(AxisNo))
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
            AxisArgsList[AxisNo].GainFactor = (int)Setting.GainFactor;

            //_controller.SetAxisPara(AxisNo +1, Setting.GainFactor, Setting.LimitP, Setting.LimitN, Setting.HomeOffset, (int)Setting.HomeMode, Setting.AxisName);

        }


        public bool SetMode(UnitID Axis, UnitSettings mode)
        {

            int AxisNo = (int)Axis - (int)UnitID.U1;
            if (!IsAxisInRange(AxisNo))
            {
                return false;
            }
            _controller.SetMode(Axis, mode);
            return true;
        }

        private bool IsPosValid(int AxisNo, double TargetPosRelative)
        {
            if (!IsAxisInRange(AxisNo))
            {
                return false;
            }
            int axisIndex = AxisNo + 1;
            var state = _controller.GetUnitState((M12.Definitions.UnitID)axisIndex);
            int TargetPosInt = (int)(TargetPosRelative * AxisArgsList[AxisNo].GainFactor) + state.AbsPosition;
            int LimtP = (int)(AxisArgsList[AxisNo].LimitP * AxisArgsList[AxisNo].GainFactor);
            int LimtN = (int)(AxisArgsList[AxisNo].LimitN * AxisArgsList[AxisNo].GainFactor);

            if (TargetPosInt > LimtP || TargetPosInt < LimtN)
                return false;
            else
                return true;
        }

        private string ParseErrorCode(int error)
        {
            if (Enum.IsDefined(typeof(EnumIrixiMotionError), error))
                return ((EnumIrixiMotionError)error).ToString();
            return "未定义错误类型";
        }

        private void OnIrixiAxisStateChanged(object sender, UnitState e)
        {
            int AxisNo = e.UnitID - (int)UnitID.U1;
            AxisArgsList[AxisNo].CurAbsPos = (double)e.AbsPosition / AxisArgsList[AxisNo].GainFactor;
            AxisArgsList[AxisNo].IsBusy = e.IsBusy;
            AxisArgsList[AxisNo].IsHomed = e.IsHomed;
            AxisArgsList[AxisNo].IsHomedAndNotBusy = e.IsHomed && (!e.IsBusy);
            AxisArgsList[AxisNo].CurAbsPosPuse = e.AbsPosition;
            OnAxisStateChanged?.Invoke(this, AxisNo, AxisArgsList[AxisNo]);
            if (AxisArgsList[AxisNo].ErrorCode != (byte)e.Error)
            {
                AxisArgsList[AxisNo].ErrorCode = (byte)e.Error;
                if ((byte)e.Error != 0)
                {
                    OnErrorOccured?.Invoke(this, (int)e.Error, ParseErrorCode((int)e.Error));
                }
            }
        }



    }


}
