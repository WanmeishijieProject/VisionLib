using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AxisParaLib
{
    public class AxisMoveArgs : INotifyPropertyChanged
    {
        private double _speed;
        private double _distance;
        private int _moveMode;
        public AxisMoveArgs()
        {
            Speed = 0;
            Distance = 0;
            MoveMode = 0;
        }
        public double Speed
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
        public double Distance
        {
            get { return _distance; }
            set
            {
                if (_distance != value)
                {
                    _distance = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int MoveMode
        {
            get { return _moveMode; }
            set
            {
                if (_moveMode != value)
                {
                    _moveMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string Name = "")
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(Name));
        }
    }
}
