using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPT_TosaTest.Vision;

namespace JPT_TosaTest.Model.ToolData
{
    public class PairToolData : ToolDataBase
    {
        public override EnumToolType ToolType { get { return EnumToolType.PairTool; } set => throw new NotImplementedException(); }
        public int CaliperNum { get; set; }
        public int ExpectPairNum { get; set; }
        public EnumPairType Polarity { get; set; }
        public EnumSelectType SelectType { get; set; }
        public int Contrast { get; set; }
        public string ModelName { set; get; }

        /// <summary>
        /// halcon的rectangle区域信息
        /// </summary>
        public string HalconDdata { set; get; }

        public override bool FromString(string ParaList)
        {
            string[] list = ParaList.Split('|');
            if (list.Count() == 3)
            {
                Enum.TryParse(list[0], out EnumToolType type);
                HalconDdata = list[2];
                if (type != EnumToolType.PairTool)
                {
                    throw new Exception($"Wrong {ToolType.ToString()} when parse {ParaList}, Please check!");
                }
                else
                {
                    var L1 = list[1].Split('&');
                    if (L1.Count() != 6)
                        throw new Exception($"Wrong para num when parse {ParaList}");
                    bool bRet = true;
                    bRet &= int.TryParse(L1[0], out int caliperNum);
                    bRet &= int.TryParse(L1[1], out int expectPairNum);
                    bRet &= Enum.TryParse(L1[2], out EnumPairType polarity);
                    bRet &= Enum.TryParse(L1[3], out EnumSelectType selectType);
                    bRet &= double.TryParse(L1[4], out double contrast);
                    string modelName = L1[5];
                    if (bRet == false)
                        throw new Exception("Error happend when parse {ParaList}");
                    else
                    {
                        this.CaliperNum = caliperNum;
                        this.ExpectPairNum = expectPairNum;
                        this.Polarity = polarity;
                        this.ModelName = modelName;
                        this.SelectType = selectType;
                        this.Contrast = (int)contrast;
                        this.HalconDdata = HalconDdata;
                        return true;
                    }
                }
            }
            else
            {
                throw new Exception($"Wrong {ToolType.ToString()} format when parse {ParaList}");
            }
        }

        public override string ToString()
        {
            return $"{ToolType.ToString()}|{CaliperNum}&{ExpectPairNum}&{Polarity.ToString()}&{SelectType.ToString()}&{Contrast}&{ModelName}|{this.HalconDdata}";
        }
    }
}
