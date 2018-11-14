using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.ProcessParaManager
{
    public class ProcessPara : INotifyPropertyChanged
    {
        private int _centerLineOffset = 2905;
        private int _padOffset=1000;
        private string _tiaModelName = "";
        private string _hsgModelName = "";
        private string _tiaType = "";


        public int CenterLineOffset
        {
            get { return _centerLineOffset; }
            set
            {
                if (value != _centerLineOffset)
                {
                    _centerLineOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int PadOffset
        {
            get { return _padOffset; }
            set
            {
                if (value != _padOffset)
                {
                    _padOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string TiaModelName
        {
            get { return _tiaModelName; }
            set
            {
                if (value != _tiaModelName)
                {
                    _tiaModelName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string HsgModelName
        {
            get { return _hsgModelName; }
            set
            {
                if (value != _hsgModelName)
                {
                    _hsgModelName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string TiaType
        {
            get { return _tiaType; }
            set
            {
                if (value != _tiaType)
                {
                    _tiaType = value;
                    RaisePropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
