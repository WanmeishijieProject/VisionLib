using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Package
{
    public interface IPackage
    {
        byte[] Header { get; set; }
        String GetPackageType();

        //供读取返回值使用
        void SetSyncFlag();
        void ResetSyncFlag();
        object ReturnObject { get; set; }
    }
}
