using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Model
{
    
    public class HardwareParaModel : INotifyPropertyChanged
    {
        private double _gainFactorX = 0.0f;
        private double _gainFactorY1 = 0.0f;
        private double _gainFactorY2 = 0.0f;
        private double _gainFactorZ = 0.0f;
        private double _gainFactorR = 0.0f;

        [ReadOnly(true)]
        [CategoryAttribute("电机参数"), DescriptionAttribute("X轴每转脉冲数")]
        public double GainFactorX
        {
            get { return _gainFactorX; }
            set
            {
                if (_gainFactorX != value)
                {
                    _gainFactorX = value;
                    RaisePropertyChanged();
                }
            }
        }

        [ReadOnly(true)]
        [CategoryAttribute("电机参数"), DescriptionAttribute("Y1轴每转脉冲数")]
        public double GainFactorY1
        {
            get { return _gainFactorY1; }
            set
            {
                if (_gainFactorY1 != value)
                {
                    _gainFactorY1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        [ReadOnly(true)]
        [CategoryAttribute("电机参数"), DescriptionAttribute("Y2轴每转脉冲数")]
        public double GainFactorY2
        {
            get { return _gainFactorY2; }
            set
            {
                if (_gainFactorY2 != value)
                {
                    _gainFactorY2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        [ReadOnly(true)]
        [CategoryAttribute("电机参数"), DescriptionAttribute("Z轴每转脉冲数")]
        public double GainFactorZ
        {
            get { return _gainFactorZ; }
            set
            {
                if (_gainFactorZ != value)
                {
                    _gainFactorZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        [ReadOnly(true)]
        [CategoryAttribute("电机参数"), DescriptionAttribute("R轴每转脉冲数")]
        public double GainFactorR
        {
            get { return _gainFactorR; }
            set
            {
                if (_gainFactorR != value)
                {
                    _gainFactorR = value;
                    RaisePropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
        }
    }
}
