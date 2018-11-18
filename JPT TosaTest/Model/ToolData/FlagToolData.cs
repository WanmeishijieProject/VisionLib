using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPT_TosaTest.Vision;

namespace JPT_TosaTest.Model.ToolData
{
    public class FlagToolDaga : ToolDataBase
    {
        public override EnumToolType ToolType { get { return EnumToolType.FlagTool; } set => throw new NotImplementedException(); }
        public EnumGeometryType GeometryType { get; set; }
        public string L1Name { get; set; }
        public string L2Name { get; set; }
        public double Halcon_Row
        {
            get
            {
                var halconDataList=HalconData.Split('&');
                if (double.TryParse(halconDataList[0], out double data))
                    return data;
                return 0;
            }
        }
        public double Halcon_Col
        {
            get
            {
                var halconDataList = HalconData.Split('&');
                if (double.TryParse(halconDataList[1], out double data))
                    return data;
                return 0;
            }
        }
        public double Halcon_Phi
        {
            get
            {
                var halconDataList = HalconData.Split('&');
                if (double.TryParse(halconDataList[2], out double data))
                    return data;
                return 0;
            }
        }

        /// <summary>
        /// Region的中心以及 中心与两条直线交点的连线与L1的夹角Phi
        /// </summary>
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
                    if (L1.Count() != 3)
                        throw new Exception($"Wrong para num when parse {ParaList}");
                    bool bRet = true;
                    bRet &= Enum.TryParse(L1[0], out EnumGeometryType geometryType);
                    string l1Name = L1[1];
                    string l2Name = L1[2];
                    if (bRet == false)
                        throw new Exception("Error happend when parse {ParaList}");
                    else
                    {
                        this.GeometryType = geometryType;
                        this.L1Name = l1Name;
                        this.L2Name = L2Name;
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
            return $"{ToolType.ToString()}|{GeometryType.ToString()}&{L1Name}&{L2Name}|{this.HalconData}";
        }
    }
}
