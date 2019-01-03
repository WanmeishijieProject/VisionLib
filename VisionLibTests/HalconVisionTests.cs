using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisionLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisionLib.CommonVisionStep;

namespace VisionLib.Tests
{
    [TestClass()]
    public class HalconVisionTests
    {
        private HalconVision Vision = HalconVision.Instance;
        [TestMethod()]
        public void FindCameraTest()
        {
            var Res= Vision.FindCamera(VisionDefinitions.EnumCamType.GigEVision, new List<string>() { "Cam_Up" }, out List<string> ErrorList);
            var Cam = Res[0];
            
            Vision.OpenCam(Cam.CamID);
            Vision.GrabImage(Cam.CamID);
            
            Vision.SaveImage(Cam.CamID, VisionDefinitions.EnumImageType.Image, "C:\\公司文件资料", "2225678.jpg", 0);
            Vision.CloseCam(Cam.CamID);
        }


    }
}