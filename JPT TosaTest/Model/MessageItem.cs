using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Model
{
    public enum EnumMessageType
    {
        Info,
        Error,
        Warning
    }
    public class MessageItem
    {
        public EnumMessageType MsgType { get; set; }
        public string StrMsg { get; set; }
    }
}
