using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.ProcessParaManager
{
    public abstract class AlignArgsBaseF : INotifyPropertyChanged
    {
        private string _argsName;
        [Browsable(false)]
        public string ArgsType => GetType().ToString();

        public string ArgsName
        {
            get { return _argsName; }
            set
            {
                if (_argsName != value)
                {
                    _argsName = value;
                    RaisePropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
