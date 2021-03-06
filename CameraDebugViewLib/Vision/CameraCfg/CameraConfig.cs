﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraDebugLib.Vision.CameraCfg
{
    public class CameraConfig
    {
        public string Name { get; set; }            //UserName:IP
        public string NameForVision { get; set; }   //Vision use
        public int LightPortChannel { get; set; }   //光源端口
        public int LightValue { get; set; }
        public string ConnectType { get; set; }
    }
}
