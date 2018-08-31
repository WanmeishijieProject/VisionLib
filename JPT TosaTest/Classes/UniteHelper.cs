using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes
{
    public enum Enumunit
    {
        m,
        mm,
        um,
        nm
    }
    public class UniteHelper
    {
        public Enumunit Unit { get; set; }
        public int GainFactor{ get; set; }  //每毫米的脉冲数
        public UniteHelper(int GainFactor,Enumunit unit=Enumunit.mm)
        {
            this.GainFactor = GainFactor;
            this.Unit = unit;
        }
        public int ToPuse(double RealDistance,out int ErrorCode)
        {
            ErrorCode = 0;
            switch (Unit)
            {
                case Enumunit.m:
                    return Convert.ToInt32((RealDistance * 1000 * GainFactor));
                case Enumunit.mm:
                    return Convert.ToInt32((RealDistance * 1 * GainFactor));
                case Enumunit.um:
                    return Convert.ToInt32(((RealDistance/1000) * GainFactor));
                case Enumunit.nm:
                    return Convert.ToInt32(((RealDistance / 1000000) * GainFactor));
                default:
                    ErrorCode = 1;
                    return 0;
            }
        }
        public double ToReal(int Puse, out int ErrorCode)
        {
            ErrorCode = 0;
            switch (Unit)
            {
                case Enumunit.m:
                    return ((double)Puse / (double)GainFactor) / 1000;
                case Enumunit.mm:
                    return ((double)Puse / (double)GainFactor) / 1;
                case Enumunit.um:
                    return ((double)Puse*1000 / (double)GainFactor);
                case Enumunit.nm:
                    return ((double)Puse * 1000000 / (double)GainFactor);
                default:
                    ErrorCode = 1;
                    return 0;
            }
        }
        
    }
}
