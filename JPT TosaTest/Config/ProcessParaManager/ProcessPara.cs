using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.ProcessParaManager
{
    public class ProcessPara : INotifyPropertyChanged
    {
        private int _centerLineOffset = 2905;
        private int _padOffset=1000;
        private string _tiaModelName = "";
        private string _hsgModelName = "";
        private string _tiaType = "";
        private double _presure = 4.0f;

        public ProcessPara()
        {
            ParaType = EnumConfigType.ProcessPara;
        }

        public EnumConfigType ParaType
        {
            get;
            private set;
        }

        public int CenterLineOffset
        {
            get { return _centerLineOffset; }
            set
            {
                if (value != _centerLineOffset)
                {
                    _centerLineOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int PadOffset
        {
            get { return _padOffset; }
            set
            {
                if (value != _padOffset)
                {
                    _padOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string TiaModelName
        {
            get { return _tiaModelName; }
            set
            {
                if (value != _tiaModelName)
                {
                    _tiaModelName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string HsgModelName
        {
            get { return _hsgModelName; }
            set
            {
                if (value != _hsgModelName)
                {
                    _hsgModelName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string TiaType
        {
            get { return _tiaType; }
            set
            {
                if (value != _tiaType)
                {
                    _tiaType = value;
                    RaisePropertyChanged();
                }
            }
        }



        public double Presure
        {
            get { return _presure; }
            set
            {
                if (value != _presure)
                {
                    _presure = value;
                    RaisePropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public override string ToString()
        {
            return $"{ParaType.ToString()}|{CenterLineOffset}&{PadOffset}&{TiaModelName}&{HsgModelName}&{Presure}";
        }
        public void FromString(string strPara)
        {
            var paraList = strPara.Split('|');
            if (paraList.Length == 2)
            {
                var strType = paraList[0];
                var L1 = paraList[1].Split('&');
                if (L1.Length == 5)
                {
                    bool bRet = true;
                    bRet &= Enum.TryParse(strType, out EnumConfigType type);
                    bRet &= int.TryParse(L1[0], out int centerLineOffset);
                    bRet &= int.TryParse(L1[1], out int padOffset);
                    bRet &= double.TryParse(L1[4], out double press);
                    if (type == EnumConfigType.ProcessPara && bRet)
                    {
                        CenterLineOffset = centerLineOffset;
                        PadOffset = padOffset;
                        TiaModelName = L1[2];
                        HsgModelName = L1[3];
                        Presure = press;
                    }
                    else
                        throw new Exception($"Wrong {ParaType.ToString()}  when parse {strPara}");
                }
                else
                    throw new Exception($"Wrong number {ParaType.ToString()}  when parse {strPara}");
            }


        }
    }
}
