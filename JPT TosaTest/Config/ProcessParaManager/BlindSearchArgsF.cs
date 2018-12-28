using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.ProcessParaManager
{
    public class BlindSearchArgsF : AlignArgsBaseF 
    {
        #region Private
        int _axisNoBaseZero;
        double _range;
        double _interval;
        double _gap;
        byte _speed;
        
        #endregion
        #region Properties

        /// <summary>
        /// Get or set the active unit.
        /// </summary>
        public int AxisNoBaseZero
        {
            get { return _axisNoBaseZero; }
            set {
                if (_axisNoBaseZero != value)
                {
                    _axisNoBaseZero = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get or set the scan range.
        /// </summary>
        public double Range
        {
            get { return _range; }
            set
            {
                if (_range != value)
                {
                    _range = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get or set the gap of the spiral curve.
        /// </summary>
        public double Gap
        {
            get { return _gap; }
            set
            {
                if (_gap != value)
                {
                    _gap = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get or set the interval of the ADC sampling trigger signal.
        /// </summary>
        public double Interval
        {
            get { return _interval; }
            set
            {
                if (_interval != value)
                {
                    _interval = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get or set the move speed.
        /// </summary>
        public byte Speed
        {
            get { return _speed; }
            set
            {
                if (_speed != value)
                {
                    _speed = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion
    }
}
