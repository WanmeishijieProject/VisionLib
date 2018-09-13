using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards
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
        public string AxisName { get; set; }
        public int AxisNo { set; get; }
        public UInt32 GainFactor { get; set; }
        public double LimitP { get; set; }
        public double LimitN { get; set; }
        public double HomeOffset { get; set; }
        public EnumHomeMode HomeMode {get;set;}


        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
