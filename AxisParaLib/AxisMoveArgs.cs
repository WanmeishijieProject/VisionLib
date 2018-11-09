using AxisParaLib.UnitManager;
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
        private UnitBase _unit = new Millimeter();
        public AxisMoveArgs()
        {
            Speed = 100;
            Distance = 1;
            MoveMode = 1;
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
        public UnitBase Unit
        {
            get { return _unit; }
            set
            {
                if (_unit != value)
                {
                    Distance = UnitHelper.ConvertUnit(_unit, value, Distance);
                    _unit = value;
                    RaisePropertyChanged();
                }
            }
        }
        public void SetUnitPrivate(UnitBase unit)
        {
            _unit = unit;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string Name = "")
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(Name));
        }
    }
}
