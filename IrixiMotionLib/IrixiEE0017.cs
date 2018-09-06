using JPT_TosaTest.MotionCards.IrixiCommand;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Package;

namespace JPT_TosaTest.MotionCards
{
    public class IrixiEE0017
    {
        private int AXIS_NUM = 12;
        private List<AxisArgs> AxisStateList = new List<AxisArgs>();
        private static object ComportLock = new object();  
        SerialPort Sp = null;
        private Queue<byte> FrameRecvByteQueue = new Queue<byte>();
        private List<short> ADCRawDataList = new List<short>();
        private static Dictionary<string, IrixiEE0017> InstanceDic = new Dictionary<string, IrixiEE0017>();

        public EventHandler<UInt16?>  OnOutputStateChanged;
        public EventHandler<Tuple<byte,AxisArgs>> OnAxisPositionChanged;


        private Irixi_Home CommandHome = new Irixi_Home();
        private Irixi_Move CommandMove = new Irixi_Move();
        private Irixi_MoveTrigger CommandMoveTrigger = new Irixi_MoveTrigger();
        private Irixi_Stop CommandStop = new Irixi_Stop();
        private Irixi_GetMcsuSta CommandGetMcsuSta = new Irixi_GetMcsuSta();
        private Irixi_GetSysSta CommandSysSta = new Irixi_GetSysSta();
        private Irixi_GetMemLen CommandGetMemLen = new Irixi_GetMemLen();
        private Irixi_ReadMem CommandReadMem = new Irixi_ReadMem();
        private Irixi_ClearMem CommandClearMem = new Irixi_ClearMem();
        private Irixi_ReadAD CommandReadAd = new Irixi_ReadAD();

        private Irixi_ConfigAdcTrig CommandConfigAdcTrigger = new Irixi_ConfigAdcTrig();
        private Irixi_SetDout CommandSetDout = new Irixi_SetDout();
        private Irixi_ReadDout CommandReadDout = new Irixi_ReadDout();
        private Irixi_ReadDin CommandReadDin = new Irixi_ReadDin();
        private Irixi_RunBlindSearch CommandRunBlindSearch = new Irixi_RunBlindSearch();


        //解析包
        private AutoResetEvent ParsePackageEvent = new AutoResetEvent(false);
        private Task TaskParsePackage = null;
        private CancellationTokenSource ctsParsePackage = null;
        private object PackageQueueLock = new object();
        private CRC32 Crc32Instance = new CRC32();
        private UInt16 PACKAGE_HEADER = 0x7E;


        //查询轴状态线程
        Task TaskAxisStateCheck = null;



        private IrixiEE0017()
        {
            for (int i = 0; i < AXIS_NUM; i++)
                AxisStateList.Add(new AxisArgs());
        }


        public static IrixiEE0017 CreateInstance(string token)
        {
            InstanceDic.TryGetValue(token, out IrixiEE0017 value);
            if (value == null)
            {
                lock (ComportLock)
                {
                    InstanceDic.TryGetValue(token, out value);
                    if (value == null)
                    {
                        InstanceDic.Add(token, new IrixiEE0017());
                    }
                }
            }
            return InstanceDic[token];
        }

        public bool Init(int ComportNo)
        {
            lock (ComportLock)
            {
                Sp = new SerialPort();
                if (Sp == null)
                    return false;
                try
                {
                    Sp.DataReceived += Comport_DataReceived1; ;
                    Sp.BaudRate = 115200;
                    Sp.PortName = $"COM{ComportNo}";
                    Sp.DataBits = 8;
                    Sp.StopBits = StopBits.One;
                    Sp.Parity = Parity.None;
                    Sp.ReadTimeout = 1000;
                    Sp.WriteTimeout = 1000;
                    //Sp.ReadTimeout = portCfg.TimeOut;
                    //Sp.WriteTimeout = portCfg.TimeOut;
                    Sp.ReceivedBytesThreshold = 1;
                    if (Sp.IsOpen)
                        Sp.Close();
                    Sp.Open();
                    if (Sp.IsOpen)
                    {
                        StartParsePackage();
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool Deinit()
        {
            lock (ComportLock)
            {
                try
                {
                    Sp.Close();
                    StopParsePackage();
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public bool GetCurrentPos(int AxisNo, out double Pos)
        {
            lock (ComportLock)
            {
                Pos = 0;
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
                {
                    if (axisArgs != null && axisArgs.ErrorCode == 0)
                    {
                        Pos = axisArgs.CurAbsPos;
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
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

            try
            {
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                CommandHome.AxisNo = (byte)AxisNo;
                CommandHome.FrameLength = 0x05;
                byte[] cmd = CommandHome.ToBytes();
                this.ExcuteCmd(cmd);
                return true;
            }
            catch
            {
                return false;
            }


        }

        public bool IsHomeStop(int AxisNo)
        {

            if (AxisNo > 12 || AxisNo < 1)
            {
                return false;
            }
            if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
            {
                if (axisArgs != null && axisArgs.ErrorCode == 0)
                {
                    return axisArgs.IsHomed && !axisArgs.IsBusy;
                }
            }
            return false;

        }

        public bool IsNormalStop(int AxisNo)
        {

            if (AxisNo > 12 || AxisNo < 1)
            {
                return false;
            }
            if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
            {
                if (axisArgs != null && axisArgs.ErrorCode == 0)
                {
                    return axisArgs.IsBusy == false;
                }
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
            try
            {
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));
                int PosTarget = Convert.ToInt32(Pos * AxisStateList[AxisNo - 1].GainFactor);
                int RelPos = 0;
                if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
                {
                    if (axisArgs != null)
                    {
                        lock (axisArgs.AxisLock)
                        {
                            if (axisArgs.ErrorCode == 0)
                            {
                                RelPos = PosTarget - Convert.ToInt32(axisArgs.CurAbsPos * axisArgs.GainFactor);
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }

                return MoveRel(AxisNo, Acc, Speed, RelPos);
            }
            catch (Exception ex)
            {
                return false;
            }

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

            try
            {
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));


                int PosTarget = Convert.ToInt32(Pos * AxisStateList[AxisNo - 1].GainFactor);
                int RelPos = 0;
                if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
                {
                    if (axisArgs != null)
                    {
                        lock (axisArgs.AxisLock)
                        {
                            if (axisArgs.ErrorCode == 0)
                            {
                                RelPos = PosTarget - Convert.ToInt32(axisArgs.CurAbsPos * axisArgs.GainFactor);
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
                return MoveRel(AxisNo, Acc, Speed, RelPos, TriggerType, Interval);
            }
            catch (Exception ex)
            {
                return false;
            }


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
            lock (ComportLock)
            {
                try
                {
                    if (AxisNo > 12 || AxisNo < 1)
                    {
                        return false;
                    }
                    var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                    byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));
                    int distancePuse = Convert.ToInt32(Distance * AxisStateList[AxisNo - 1].GainFactor);
                    CommandMove.FrameLength = 0x0C;
                    CommandMove.AxisNo = (byte)AxisNo;
                    CommandMove.Distance = distancePuse;
                    CommandMove.SpeedPercent = speedPer;
                    byte[] cmd = CommandMove.ToBytes();
                    this.ExcuteCmd(cmd);
                    CheckAxisState(Enumcmd.Move, AxisNo - 1);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
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
            lock (ComportLock)
            {
                try
                {
                    if (AxisNo > 12 || AxisNo < 1)
                    {
                        return false;
                    }
                    var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                    byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));
                    int distancePuse = Convert.ToInt32(Distance * AxisStateList[AxisNo - 1].GainFactor);
                    CommandMoveTrigger.FrameLength = 0x0E;
                    CommandMoveTrigger.TriggerType = TriggerType == EnumTriggerType.ADC ? Enumcmd.MoveTrigAdc : Enumcmd.MoveTrigOut;
                    CommandMoveTrigger.AxisNo = (byte)AxisNo;
                    CommandMoveTrigger.Distance = distancePuse;
                    CommandMoveTrigger.SpeedPercent = speedPer;
                    CommandMoveTrigger.TriggerInterval = Interval;
                    byte[] cmd = CommandMoveTrigger.ToBytes();
                    this.ExcuteCmd(cmd);
                    CheckAxisState(Enumcmd.Move, AxisNo - 1);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

        }

        public bool ReadIoInBit(int Index, out bool value)
        {
            lock (ComportLock)
            {
                value = false;
                bool bRet = ReadIoInWord(1, out int wordValue);
                value = (wordValue & (1 << (Index - 1))) == 1;
                return bRet;
            }
        }

        public bool ReadIoInWord(int StartIndex, out int value)
        {
            lock (ComportLock)
            {
                value = 0;
                try
                {
                    CommandReadDin.FrameLength = 0x04;
                    byte[] cmd = CommandReadDin.ToBytes();
                    this.ExcuteCmd(cmd);
                    bool bRet = CommandReadDin.WaitFinish(3000);
                    bRet &= Int32.TryParse(CommandReadDin.ReturnObject.ToString(), out value);
                    return bRet;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool ReadIoOutBit(int Index, out bool value)
        {
            lock (ComportLock)
            {
                value = false;
                try
                {
                    bool bRet = ReadIoOutWord(1, out int wordValue);
                    value = (wordValue & (1 << (Index - 1))) != 0;
                    return bRet;
                }
                catch
                {
                    return false;
                }
            }

        }

        public bool ReadIoOutWord(int StartIndex, out int value)
        {
            lock (ComportLock)
            {
                value = 0;
                try
                {
                    CommandReadDout.FrameLength = 0x04;
                    byte[] cmd = CommandReadDout.ToBytes();
                    lock (ComportLock)
                    {
                        this.ExcuteCmd(cmd);
                    }
                    bool bRet = CommandReadDout.WaitFinish(3000);
                    Console.WriteLine("---------ReadOutWaitOne-----------");
                    bRet &= Int32.TryParse(CommandReadDout.ReturnObject.ToString(), out value);
                    return bRet;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool WriteIoOutBit(int Index, bool value)
        {
            lock (ComportLock)
            {
                try
                {
                    CommandSetDout.FrameLength = 0x06;
                    CommandSetDout.GPIOChannel = (byte)Index;
                    CommandSetDout.GPIOState = value ? (byte)1 : (byte)0;
                    byte[] cmd = CommandSetDout.ToBytes();
                    this.ExcuteCmd(cmd);
                    UInt16? data = null;
                    if (ReadIoOutWord(Index, out int outputValue))
                    {
                        data = (UInt16)outputValue;
                    }
                    OnOutputStateChanged?.Invoke(this, data);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool WriteIoOutWord(int StartIndex, ushort value)
        {
            try
            {
                for (int i = StartIndex; i < StartIndex + 8; i++)
                {
                    lock (ComportLock)
                    {
                        byte GPIOChannel = (byte)StartIndex;
                        byte GPIOState = (byte)((value >> (i - StartIndex)) & 0x01);
                        WriteIoOutBit(GPIOChannel, GPIOState!=0);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// 配置Trigger
        /// </summary>
        /// <param name="ChannelFlags"></param>
        /// <returns></returns>
        public bool SetTrigConfig(byte ChannelFlags)
        {
            lock (ComportLock)
            {
                try
                {
                    CommandConfigAdcTrigger.FrameLength = 0x05;
                    CommandConfigAdcTrigger.ADCChannelFlags = ChannelFlags;
                    byte[] cmd = CommandConfigAdcTrigger.ToBytes();
                    this.ExcuteCmd(cmd);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool GetMemLength(out UInt32 Length)
        {
            lock (ComportLock)
            {
                Length = 0;
                try
                {
                    CommandGetMemLen.FrameLength = 0x04;
                    byte[] cmd = CommandGetMemLen.ToBytes();
                    this.ExcuteCmd(cmd);
                    CommandGetMemLen.WaitFinish(1000);
                    Length = (UInt32)CommandGetMemLen.ReturnObject;
                    return true;

                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool ReadMem(UInt32 Offset, UInt32 Length, out List<Int16> RawDataList)
        {
            lock (ComportLock)
            {
                RawDataList = null;
                try
                {
                    ADCRawDataList.Clear();
                    CommandReadMem.FrameLength = 0x0C;
                    CommandReadMem.MemOffset = Offset;
                    CommandReadMem.MemLength = Length;
                    byte[] cmd = CommandReadMem.ToBytes();
                    Sp.Write(cmd, 0, cmd.Length);
                    CommandReadMem.WaitFinish(10000);
                    RawDataList = CommandReadMem.ReturnObject as List<Int16>;
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

        }

        public bool ClearMem()
        {
            lock (ComportLock)
            {
                try
                {
                    CommandClearMem.FrameLength = 0x04;
                    byte[] cmd = CommandClearMem.ToBytes();
                    Sp.Write(cmd, 0, cmd.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool ReadAD(byte ChannelFlags)
        {
            lock (ComportLock)
            {
                try
                {
                    CommandReadAd.FrameLength = 0x05;
                    CommandReadAd.ADChannelFlags = ChannelFlags;
                    byte[] cmd = CommandReadAd.ToBytes();
                    this.ExcuteCmd(cmd);
                    return true;

                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool SetCurrentPos(int AxisNo, double Pos)
        {
            if (AxisNo > 12 || AxisNo < 1)
            {
                return false;
            }
            return false;
        }

        public bool Stop()
        {
            lock (ComportLock)
            {
                try
                {
                    CommandStop.FrameLength = 0x05;
                    CommandStop.AxisNo = (byte)0x00;
                    byte[] cmd = CommandStop.ToBytes();
                    this.ExcuteCmd(cmd);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool DoBindSearch(uint HAxis, uint VAxis, double Range, double Gap, double Speed, double Interval)
        {
            lock (ComportLock)
            {
                try
                {
                    CommandRunBlindSearch.FrameLength = 0x11;
                    CommandRunBlindSearch.HAxisNo = (byte)HAxis;
                    CommandRunBlindSearch.VAxisNo = (byte)VAxis;
                    int AxisIndex = (int)HAxis;
                    CommandRunBlindSearch.Range = (uint)(Range * AxisStateList[AxisIndex].GainFactor);
                    CommandRunBlindSearch.Gap = (uint)(Gap * AxisStateList[AxisIndex].GainFactor);
                    CommandRunBlindSearch.SpeedPercent = (byte)((Speed * AxisStateList[AxisIndex].GainFactor) / 10000);
                    CommandRunBlindSearch.Interval = (UInt16)(Interval * AxisStateList[AxisIndex].GainFactor);
                    byte[] cmd = CommandStop.ToBytes();
                    this.ExcuteCmd(cmd);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }


        #region Private
        private bool GetMcsuState(int AxisNo, out AxisArgs axisargs)
        {
            lock (ComportLock)
            {
                axisargs = null;
                try
                {
                    if (AxisNo > 12 || AxisNo < 1)
                    {
                        return false;
                    }
                    //Command
                    CommandGetMcsuSta.FrameLength = 0x05;
                    CommandGetMcsuSta.AxisNo = (byte)AxisNo;
                    byte[] cmd = CommandGetMcsuSta.ToBytes();
                    this.ExcuteCmd(cmd);
                    CommandGetMcsuSta.WaitFinish(1000);
                    axisargs = AxisStateList[AxisNo - 1];
                    return axisargs != null;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }



        private void Comport_DataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            int Len = Sp.BytesToRead;
            for (int i = 0; i < Len; i++)
            {
                byte bt = (byte)Sp.ReadByte();
                lock (PackageQueueLock)
                {
                    FrameRecvByteQueue.Enqueue(bt);
                    //if (bt == PACKAGE_HEADER)
                    //    ParsePackageEvent.Set();
                }
            }
        }

        private byte CheckSum(byte[] buf, int offset, int count)
        {
            if (buf.Length < count)
                return 0;
            byte checksum = 0;
            for (int i = offset; i < offset + count; i++)
            {
                checksum += buf[i];
            }
            return (byte)(checksum & 0xFF);
        }

        private void ExcuteCmd(byte[] Cmd)
        {

            Sp.Write(Cmd, 0, Cmd.Length);

        }
     
        private void StartParsePackage()
        {
            long TickStart = 0;
            if (TaskParsePackage == null || TaskParsePackage.IsCanceled || TaskParsePackage.IsCompleted)
            {
                ctsParsePackage = new CancellationTokenSource();
                
                TaskParsePackage = new Task(() =>
                {
                    TickStart = DateTime.Now.Ticks;
                    List<byte> TempList = new List<byte>();
                    int ExpectLength = 0;
                    while (!ctsParsePackage.IsCancellationRequested)
                    {
                        Thread.Sleep(1);
                        //ParsePackageEvent.WaitOne();    用事件阻塞小行程延迟比较厉害
                        if (FrameRecvByteQueue.Count > 0)
                        {
                            byte data = 0x00;
                            lock (PackageQueueLock)
                            {
                                try
                                {
                                    data = FrameRecvByteQueue.Dequeue();
                                }
                                catch (InvalidOperationException ex)
                                {
                                    continue;
                                }
                            }
                            if (data == PACKAGE_HEADER && TempList.Count == 0)
                            {
                                TempList.Add(data);
                            }
                            else if (TempList.Count > 0)
                            {
                                TempList.Add(data);
                                if (TempList.Count == 3)
                                {
                                    ExpectLength = TempList[1] + (TempList[2] << 8);
                                }
                                else if (ExpectLength > 0)
                                {
                                    if (TempList.Count == ExpectLength + 7)
                                    {
                                        byte[] dataList = TempList.ToArray();
                                        UInt32 Crc32 = (UInt32)(dataList[dataList.Length - 4] + (dataList[dataList.Length - 3] << 8) + (dataList[dataList.Length - 2] << 16) + (dataList[dataList.Length - 1] << 24));
                                        UInt32 CalcCrc32 = Crc32Instance.Calculate(dataList, 0, dataList.Length-4);
                                        if (Crc32 == CalcCrc32) //校验成功
                                        {
                                            ProcessPackage(dataList);
                                        }
                                        ExpectLength = 0;
                                        TempList = new List<byte>();
                                    }
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                
                }, ctsParsePackage.Token);
                TaskParsePackage.Start();
            }
        }
        //处理收到的包
        private void ProcessPackage(byte[] data)
        {
            byte Cmd = data[6];
            int RealLen = data.Length;
            switch (Cmd)
            {
                case (byte)Enumcmd.GetMcsuSta:      //读取状态值
                    CommandGetMcsuSta.ByteArrToPackage(data);
                    MCSUS_STATE returnValue = CommandGetMcsuSta.ReturnObject as MCSUS_STATE;
                    int axisIndex = returnValue.AxisIndex;
                    lock (AxisStateList[axisIndex - 1].AxisLock)
                    {
                        AxisStateList[axisIndex - 1].IsHomed = returnValue.IsHomed;
                        AxisStateList[axisIndex - 1].IsBusy = returnValue.IsBusy;
                        AxisStateList[axisIndex - 1].ErrorCode = returnValue.Error;
                        AxisStateList[axisIndex - 1].CurAbsPos = (double)returnValue.AbsPosition / (double)AxisStateList[axisIndex - 1].GainFactor;
                    }
                    CommandGetMcsuSta.SetSyncFlag();
                    break;
                case (byte)Enumcmd.GetMemLength:
                    CommandGetMemLen.ByteArrToPackage(data);
                    CommandGetMemLen.SetSyncFlag();
                    break;
                case (byte)Enumcmd.ReadMem:
                    CommandReadMem.ByteArrToPackage(data);
                    CommandReadMem.SetSyncFlag();

                    break;
                case (byte)Enumcmd.ReadDin:
                    CommandReadDin.ByteArrToPackage(data);  //解析包
                    CommandReadDin.SetSyncFlag();   //通知读取完毕
                    Console.WriteLine("---------ReadInOver-----------");
                    break;
                case (byte)Enumcmd.ReadDout:
                    CommandReadDout.ByteArrToPackage(data);
                    CommandReadDout.SetSyncFlag();
                    
                    Console.WriteLine("---------ReadOutSetOver-----------");
                    break;
            }

        }

        private void StopParsePackage()
        {
            ParsePackageEvent.Set();
            if (ctsParsePackage != null)
                ctsParsePackage.Cancel();
        }


        //触发轴状态事件
        private void CheckAxisState(Enumcmd Command,int Index)
        {
            AxisStateList[Index].ReqStartTime = DateTime.Now.Ticks;
            if (TaskAxisStateCheck == null || TaskAxisStateCheck.IsCanceled || TaskAxisStateCheck.IsCompleted)
            {
                AxisStateList[Index].IsInRequest = true;
                TaskAxisStateCheck=new Task(()=> {              
                    while (true)
                    {
                        if (this.GetMcsuState(Index + 1, out AxisArgs state))   //更新状态
                        {
                            OnAxisPositionChanged?.Invoke(this, new Tuple<byte, AxisArgs>((byte)(Index + 1), state));
                        }
                        Thread.Sleep(200);
                        if (TimeSpan.FromTicks(DateTime.Now.Ticks - AxisStateList[Index].ReqStartTime).TotalSeconds > 100)
                        {
                            break; ;
                        }

                        if (Command == Enumcmd.Home)
                        {
                            if (!AxisStateList[Index].IsBusy && AxisStateList[Index].IsHomed)
                                break;
                        }
                        else if (Command == Enumcmd.Move || Command == Enumcmd.MoveTrigAdc || Command == Enumcmd.MoveTrigOut)
                        {
                            if (!AxisStateList[Index].IsBusy)
                                break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    AxisStateList[Index].IsInRequest = false;
                });
                TaskAxisStateCheck.Start();
            }
        }
       
        #endregion


    }
}
