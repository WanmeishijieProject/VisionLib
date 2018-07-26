using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPT_TosaTest.Classes;
namespace UnitTest
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestGetDllInfo()
        {
            StringBuilder Name=new StringBuilder(),Version=new StringBuilder();
            PSSWraper.GetDllInfo(Name, Version);
            Console.WriteLine($"Name:{Name}, Version:{Version}");
        }

        [Test]
        public void TestGetLIV_5_DllInfo()
        {
            StringBuilder Name = new StringBuilder(), Version = new StringBuilder();
            PSSWraper.GetLIV_5_DllInfo(Name, Version);
            Console.WriteLine($"Name:{Name}, Version:{Version}");
        }
    }
}
