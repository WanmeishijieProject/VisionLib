using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPT_TosaTest.Vision;

namespace JPT_TosaTest.Model.ToolData
{
    public class LineToolData : ToolDataBase
    {

        public override EnumToolType ToolType { get { return EnumToolType.LineTool; } set { } }
        public int CaliperNum { get; set; }
        public EnumEdgeType Polarity { get; set; }
        public EnumSelectType SelectType { get; set; }
        public int Contrast { get; set; }
        public string ModelName { get; set; }
        public string HalconData { get; set; }

        public override bool FromString(string ParaList)
        {
            string[] list = ParaList.Split('|');
            if (list.Count() == 3)
            {
                Enum.TryParse(list[0], out EnumToolType type);
                HalconData = list[2];
                if (type != EnumToolType.LineTool)
                {
                    throw new Exception($"Wrong {ToolType.ToString()} when parse {ParaList}, Please check!");
                }
                else
                {
                    var L1 = list[1].Split('&');
                    if (L1.Count() != 5)
                        throw new Exception($"Wrong para num when parse {ParaList}");
                    bool bRet = true;
                    bRet &= int.TryParse(L1[0], out int caliperNum);
                    bRet &= Enum.TryParse(L1[1], out EnumEdgeType polarity);
                    bRet &= Enum.TryParse(L1[2], out EnumSelectType selectType);
                    bRet &= double.TryParse(L1[3], out double contrast);
                    string modelName = L1[4];
                    if (bRet == false)
                        throw new Exception("Error happend when parse {ParaList}");
                    else
                    {
                        this.CaliperNum = caliperNum;
                        this.Polarity = polarity;
                        this.ModelName = modelName;
                        this.SelectType = selectType;
                        this.Contrast = (int)contrast;
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
            return $"{ToolType.ToString()}|{CaliperNum}&{Polarity.ToString()}&{SelectType.ToString()}&{Contrast}&{ModelName}|{HalconData}";
        }
        
    }
}
