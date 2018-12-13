using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards
{
    public class M12Wrapper : M12.Controller
    {
        private static Dictionary<string, M12Wrapper> InstanseDic = new Dictionary<string, M12Wrapper>();
        
        private M12Wrapper(string PortName, int Baudrate) : base(PortName, Baudrate)
        {

        }
        public static M12Wrapper CreateInstance(string PortName, int Baudrate)
        {
            if (InstanseDic.Keys.Contains(PortName))
                return InstanseDic[PortName];
            else
            {
                var ins= new M12Wrapper(PortName, Baudrate);
                InstanseDic.Add(PortName,ins);
                return ins;
            }
        }


        
    }
}
