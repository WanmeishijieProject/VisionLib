using AxisParaLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{
    /// <summary>
    /// 回原点方式
    /// </summary>
    public enum EnumHomeMode : int
    {
        O,
        P,
        N,
    }

    /// <summary>
    /// 轴的基本设置
    /// </summary>
    public class AxisSetting : INotifyPropertyChanged
    {
        public AxisSetting()
        {
            AxisType = EnumAxisType.LinearAxis;
        }

        public string AxisName { get; set; }
        public int AxisNo { set; get; }
        public UInt32 GainFactor { get; set; }
        public double LimitP { get; set; }
        public double LimitN { get; set; }
        public double HomeOffset { get; set; }
        public EnumHomeMode HomeMode {get;set;}
        public EnumAxisType AxisType { get; set; }
        public string ForwardCaption { get; set; }
        public string BackwardCaption { get; set; }
        public int MaxSpeed { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
