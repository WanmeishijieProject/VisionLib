using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Vision.VisionToolModel
{
    interface ITool
    {
        RelayCommand RunCommand { get; set; }
    }
}
