using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AxisParaLib
{

    public class AxisArgs : INotifyPropertyChanged
    {
        private double _curAbsPos;
        private bool _isHomed;
        private bool _isBusy;
        private byte _errorCode;
        private int _gainFactor;
        private bool _limitPTrigged;
        private bool _limitNTrigged;
        private bool _originTrigged;

        public AxisArgs()
        {
            
            CurAbsPos = 0;
            IsHomed = false;
            IsBusy = false;

            GainFactor = 1;
            AxisLock = new object();
            TimeOut = 10;
            IsInRequest = false;
            MoveArgs = new AxisMoveArgs();

        }
        public double CurAbsPos
        {
            get
            {
                return _curAbsPos;
            }
            set
            {
                UpdateProperty(ref _curAbsPos, value);
            }
        }
        public bool IsHomed
        {
            get
            {
                return _isHomed;
            }
            set
            {
                UpdateProperty(ref _isHomed, value);
            }
        }
        public bool LimitPTrigged
        {
            get
            {
                return _limitPTrigged;
            }
            set
            {
                UpdateProperty(ref _limitPTrigged, value);
            }
        }

        public bool LimitNTrigged
        {
            get
            {
                return _limitNTrigged;
            }
            set
            {
                UpdateProperty(ref _limitNTrigged, value);
            }
        }
        public bool OriginTrigged
        {
            get
            {
                return _originTrigged;
            }
            set
            {
                UpdateProperty(ref _originTrigged, value);
            }
        }
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                UpdateProperty(ref _isBusy, value);
            }
        }
        public byte ErrorCode
        {
            get
            {
                return _errorCode;
            }
            set
            {
                if (ErrorCode != value)
                {
                    LimitNTrigged = (value == 6);
                    LimitPTrigged = (value == 5);
                }
                UpdateProperty(ref _errorCode, value);
            }
        }


        public int GainFactor

        {
            get
            {
                return _gainFactor;
            }
            set
            {
                UpdateProperty(ref _gainFactor, value);
            }
        }
        public bool IsInRequest { get; set; }
        public long ReqStartTime { get; set; }
        public int TimeOut { get; set; }  
        public double LimitP { get; set; }
        public double LimitN { get; set; }
        public double HomeOffset { get; set; }
        public int HomeMode
        {
            get;
            set;
        }
        public string AxisName
        {
            get;set;
        }
        public int AxisNo
        {
            get;set;
        }
        public AxisMoveArgs MoveArgs
        {
            get;set;
        }

        public object AxisLock { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        #region RaisePropertyChangedEvent
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        /// <param name="PropertyName"></param>
        protected void UpdateProperty<T>(ref T OldValue, T NewValue, [CallerMemberName]string PropertyName = "")
        {
            if (object.Equals(OldValue, NewValue))  // To save resource, if the value is not changed, do not raise the notify event
                return;

            OldValue = NewValue;                // Set the property value to the new value
            OnPropertyChanged(PropertyName);    // Raise the notify event
        }

        protected void OnPropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion
    }
}
