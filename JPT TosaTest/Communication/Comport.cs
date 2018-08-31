using JPT_TosaTest.Config.HardwareManager;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Communication
{
   
    public class Comport : CommunicationPortBase
    {
        private SerialPort sp = new SerialPort();
        private object ComportLock = new object();
        public delegate void DataReceived(SerialPort sp);
        public event DataReceived OnDataReceived;
        public ComportCfg portCfg = null;

        public Comport(ICommunicationPortCfg portCfg) : base(portCfg)
        {
            this.portCfg = portCfg as ComportCfg;
            sp.ReceivedBytesThreshold = 1;
            sp.DataReceived += Sp_DataReceived;
        }

        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            OnDataReceived?.Invoke(sp);
        }
        public int Read(byte[] buffer, int offset, int count)
        {
            lock (ComportLock)
            {
                return sp.Read(buffer, offset, count);
            }
        }
        public void Write(byte[] buffer, int offset, int count)
        {
            lock (ComportLock)
            {
                sp.Write(buffer, offset, count);
            }
        }

        public override bool OpenPort()
        {
            try

            {
                sp.BaudRate = portCfg.BaudRate;
                sp.PortName = portCfg.Port;
                sp.DataBits = portCfg.DataBits;
                switch (portCfg.StopBits)
                {
                    case 0:
                        sp.StopBits = StopBits.None;
                        break;
                    case 1:
                        sp.StopBits = StopBits.One;
                        break;
                    case 2:
                        sp.StopBits = StopBits.Two;
                        break;
                    default:
                        sp.StopBits = StopBits.OnePointFive;
                        break;
                }
                switch (portCfg.Parity.ToLower())
                {
                    case "n":
                        sp.Parity = Parity.None;
                        break;
                    case "e":
                        sp.Parity = Parity.Even;
                        break;
                    case "o":
                        sp.Parity = Parity.Odd;
                        break;

                }
                sp.ReadTimeout = portCfg.TimeOut;
                sp.WriteTimeout = portCfg.TimeOut;
                sp.ReceivedBytesThreshold = 1;
                if (sp.IsOpen)
                    sp.Close();
                sp.Open();
                return sp.IsOpen;
            }
            catch
            {
                return false;
            }

        }

        public override bool ClosePort()
        {
            try
            {
                sp.Close();
                return !sp.IsOpen;
            }
            catch
            {
                return false;
            }
        }
        public override bool IsOpen()
        {
            lock (ComportLock)
            {
                return sp.IsOpen;
            }
        }
    }
}
