using JPT_TosaTest.Classes.AlimentClass.AlimentArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.ProcessParaManager
{
    public class ProcessParaMgr
    {
        public BlindSearchArgsF[] BlindSearArgs {get; set;}
        public string CurBlindSearchHArgsName { get; set; }
        public string CurBlindSearchVArgsName { get; set; }

        public void GetBlindSearchArgs(out BlindSearchArgsF HArgs, out BlindSearchArgsF VArgs)
        {
            HArgs = VArgs = null;
            var args = from arg in this.BlindSearArgs where arg.ArgsName == CurBlindSearchHArgsName select arg;
            if (args.Count() != 0)
                HArgs= args.First();
            args = from arg in this.BlindSearArgs where arg.ArgsName == CurBlindSearchVArgsName select arg;
            if (args.Count() != 0)
                VArgs = args.First();
            if (HArgs == null || VArgs == null)
                throw new Exception($"{CurBlindSearchHArgsName} or {CurBlindSearchVArgsName} is not exist!");
        }
       

    }
}
