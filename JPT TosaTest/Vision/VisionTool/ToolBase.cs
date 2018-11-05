using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;

namespace JPT_TosaTest.Vision.VisionTool
{
    public abstract class ToolBase : INotifyPropertyChanged
    {
        public abstract string DefaultPath { get; set; }
        protected void RaisePropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
       

    }
}
