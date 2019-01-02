using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VisionLib.VisionDefinitions;

namespace VisionLib
{
    public class FindCamResult
    {
        public FindCamResult(string name,string visionName, EnumCamType type)
        {
            ActualName = name;
            NameForVision = visionName;
            Type = type;
        }

        /// <summary>
        /// 通常是用户自己定义的名称
        /// </summary>
        public string ActualName { get; set; }

        /// <summary>
        /// 打开相机用到的参数
        /// </summary>
        public string NameForVision { get; set; }

        /// <summary>
        /// 相机的类型
        /// </summary>
        public EnumCamType Type { get; set; }
    }
}
