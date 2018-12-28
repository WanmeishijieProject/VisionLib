using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.WatchDog
{
    public class Dog
    {
        private int TimeOut;
        private long StartTime;
        public Dog(int Timeout = 1000)
        {
            this.TimeOut = Timeout;
            StartTime = DateTime.Now.Ticks;
        }

        public void CheckTimeOut(string StrContent="")
        {
            if (TimeSpan.FromTicks(DateTime.Now.Ticks - StartTime).TotalMilliseconds > TimeOut)
            {
                if(string.IsNullOrEmpty(StrContent))
                    throw new Exception("Timeout for waiting");
                else
                    throw new Exception(StrContent);
            }
                
        }
    }
}
