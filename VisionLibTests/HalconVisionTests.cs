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
        [TestMethod()]
        public void FindCameraTest()
        {
            var Res=HalconVision.Instance.FindCamera(VisionDefinitions.EnumCamType.GigEVision, new List<string>() { "Cam_Up" }, out List<string> ErrorList);

            StepFindModel FindModel = new StepFindModel()
            {
                In_CamID = 1,
            };
            Console.WriteLine($"{Res[0].NameForVision}");
        }


    }
}