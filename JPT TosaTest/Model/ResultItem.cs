using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Model
{
    public class ResultItem : INotifyPropertyChanged
    {
        private int _index;
        public int Index
        {
            set
            {
                if (_index != value)
                {
                    _index = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Index"));
                }
            }
            get { return _index; }
        }

        private double _hsg_X;
        public double HSG_X
        {
            set
            {
                if (_hsg_X != value)
                {
                    _hsg_X = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HSG_X"));
                }
            }
            get { return _hsg_X; }
        }

        private double _hsg_Y;
        public double HSG_Y
        {
            set
            {
                if (_hsg_Y != value)
                {
                    _hsg_Y = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HSG_Y"));
                }
            }
            get { return _hsg_Y; }
        }

        private double _hsg_R;
        public double HSG_R
        {
            set
            {
                if (_hsg_R != value)
                {
                    _hsg_R = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HSG_R"));
                }
            }
            get { return _hsg_R; }
        }

        private double _plc_X;
        public double PLC_X
        {
            set
            {
                if (_plc_X != value)
                {
                    _plc_X = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PLC_X"));
                }
            }
            get { return _plc_X; }
        }

        private double _plc_Y;
        public double PLC_Y
        {
            set
            {
                if (_plc_Y != value)
                {
                    _plc_Y = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PLC_Y"));
                }
            }
            get { return _plc_Y; }
        }

        private double _plc_R;
        public double PLC_R
        {
            set
            {
                if (_plc_R != value)
                {
                    _plc_R = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PLC_R"));
                }
            }
            get { return _plc_R; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

  
    }
}
