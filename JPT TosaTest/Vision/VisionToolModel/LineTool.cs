using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Vision.VisionToolModel
{
    public class LineTool : ToolBase , INotifyPropertyChanged
    {
        private int _caliperNumber;
        private int _contrast;
        private int _polarity;  //light/dark, dark/light, all
        private int _lineType;  //first，last, all

        public int CaliperNumber
        {
            get { return _caliperNumber; }
            set
            {
                if (value != _caliperNumber)
                {
                    _caliperNumber = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int Polarity
        {
            get { return _polarity; }
            set
            {
                if (value != _polarity)
                {
                    _polarity = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int Contrast
        {

            get { return _contrast; }
            set
            {
                if (value != _contrast)
                {
                    _contrast = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int LineType
        {
            get { return _lineType; }
            set
            {
                if (value != _lineType)
                {
                    _lineType = value;
                    RaisePropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(PropertyName));
        }
    }
}
