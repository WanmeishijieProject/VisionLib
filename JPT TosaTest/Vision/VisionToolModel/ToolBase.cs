using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;

namespace JPT_TosaTest.Vision.VisionToolModel
{
    public abstract class ToolBase : ITool
    {
        public RelayCommand RunCommand { get; set; }

    }
}
