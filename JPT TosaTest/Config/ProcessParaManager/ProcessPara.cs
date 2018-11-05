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

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
