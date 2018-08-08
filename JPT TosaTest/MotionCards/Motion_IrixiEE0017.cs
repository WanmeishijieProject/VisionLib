using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrixiStepperControllerHelper;

namespace JPT_TosaTest.MotionCards
{
    public struct EnumAxisState
    {
        public double CurAbsPos;
        public bool IsHomed;
    }


    public class Motion_IrixiEE0017 : MotionBase
    {
       
        private IrixiMotionController _controller = null;
        private bool IsInitialized = false;
        

        public override bool Init(int minAxis, int maxAxis, int gainFactor)
        {
            MAX_AXIS = maxAxis;
            MIN_AXIS = minAxis;

            _controller = new IrixiMotionController();
            _controller.Open();
            _controller.OnConnectionStatusChanged += _controller_OnConnectionProgressChanged;
            _controller.OnReportUpdated += _controller_OnReportUpdated;

            return _controller.IsConnected;
        }

        public override bool Deinit()
        {
            try
            {
                _controller.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _controller = null;
            }
        }

        public override bool GetCurrentPos(int AxisNo, out double Pos)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 回原点，阻塞函数
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <param name="Dir"></param>
        /// <param name="Acc"></param>
        /// <param name="Speed1"></param>
        /// <param name="Speed2"></param>
        /// <returns></returns>
        public override bool Home(int AxisNo, int Dir, double Acc, double Speed1, double Speed2)    //阻塞
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            // if the controller is not connected, return
            else if (!IsInitialized)
            {
                return false;
            }
            try
            {
                return _controller.Home(AxisNo - MIN_AXIS);
                
            }
            catch (Exception ex)
            {
                return false;
            }

        }



        public override bool IsHomeStop(int AxisNo)
        {
            throw new NotImplementedException();
        }

        public override bool IsNormalStop(int AxisNo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 绝对运动，阻塞
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴号</param>
        /// <param name="Acc">绝对运动加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Pos">绝对位置</param>
        /// <returns></returns>
        public override bool MoveAbs(int AxisNo, double Acc, double Speed, double Pos)  //阻塞
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            return _controller.Move(AxisNo - MIN_AXIS,Convert.ToInt32(Speed * 1000), Convert.ToInt32(Pos * 1000), MoveMode.ABS);
        }

        /// <summary>
        /// 相对运动,阻塞
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴</param>
        /// <param name="Acc">加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Distance">相对距离</param>
        /// <returns></returns>
        public override bool MoveRel(int AxisNo, double Acc, double Speed, double Distance)
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            return _controller.Move(AxisNo - MIN_AXIS, Convert.ToInt32(Speed * 1000), Convert.ToInt32(Distance * 1000), MoveMode.REL);
        }

        public override bool SetCurrentPos(int AxisNo, double Pos)
        {
            throw new NotImplementedException();
        }

        public override bool Stop()
        {
            if (this.IsInitialized)
                return _controller.Stop(-1);  // stop all, whatever it is moving or not
            return false;
        }

        private void _controller_OnConnectionProgressChanged(object sender, ConnectionEventArgs e)
        {
            switch (e.Event)
            {
                case ConnectionEventArgs.EventType.ConnectionSuccess:
                    this.IsInitialized = true;
                    break;

                case ConnectionEventArgs.EventType.ConnectionLost:
                    this.IsInitialized = false;
                    break;
            }
        }
        private void _controller_OnReportUpdated(object sender, DeviceStateReport e)
        {
            foreach (var state in e.AxisStateCollection)
            {       
                //_axis.AbsPosition = state.AbsPosition;
                //_axis.IsHomed = state.IsHomed;
            }
        }
    }
}
