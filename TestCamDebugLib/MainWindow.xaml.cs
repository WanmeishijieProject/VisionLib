using CameraDebugLib.Vision.CameraCfg;
using System.Collections.Generic;
using System.Windows;
using TestCamDebugLib.ViewModel;

namespace TestCamDebugLib
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// 
        public List<CameraConfig> CamList { set; get; }
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            CamList = new List<CameraConfig>()
            {
                new CameraConfig()
                {
                     ConnectType="GigEVision",
                     LightPortChannel=3,
                     LightValue=55,
                     Name="Up",
                     NameForVision="CamLocal"
                }
            };
        }
    }
}