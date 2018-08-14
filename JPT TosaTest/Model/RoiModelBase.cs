using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Models
{
    public class RoiModelBase
    {
        public virtual string StrName { get; set; }
        public virtual string StrFullName { get; set; }
        public virtual int Index { get; set; }
        public virtual RelayCommand<RoiModelBase> OperateAdd
        {
            get;
        }
        public virtual RelayCommand<RoiModelBase> OperateDelete
        {
            get;
        }
    }
}
