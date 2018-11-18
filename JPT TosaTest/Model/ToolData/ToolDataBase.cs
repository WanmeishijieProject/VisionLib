using JPT_TosaTest.Vision;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Model.ToolData
{
    public abstract class ToolDataBase : INotifyPropertyChanged
    {
        public abstract EnumToolType ToolType { get; set; }
        public abstract bool FromString(string ParaList);
 



        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string PropertyName="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

    }
}
