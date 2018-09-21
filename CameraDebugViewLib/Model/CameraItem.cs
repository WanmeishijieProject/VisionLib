using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraDebugLib.Model 
{
    public class CameraItem : INotifyPropertyChanged
    {
        private string _strCameraState = "";
        private string _cameraName = "";
        public string StrCameraState
        {
            set
            {
                if (_strCameraState != value)
                {
                    _strCameraState = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StrCameraState"));
                }
            }
            get { return _strCameraState; }
        }
        public string CameraName
        {
            set
            {
                if (_cameraName != value)
                {
                    _cameraName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CameraName"));
                }
            }
            get { return _cameraName; }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
