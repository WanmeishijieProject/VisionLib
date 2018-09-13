using System;
using System.Data;
using System.Configuration;
using System.Web;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.POIFS.FileSystem;
using System.IO;
namespace JPT_TosaTest.Classes
{
    /// <summary>
    /// Ricky.Sun
    /// 2018年4月6日
    /// 基于NPOI的Excel简单读取写入类，还有待完善
    /// </summary>
    public class LogExcel
    {

        private string FileName = null; //文件名  

        public LogExcel()
        {
            
        }

        public bool CreateExcelFile(string[] colName, string fileName, string sheetName = "Sheet1")
        {
            try
            {
                this.FileName = fileName;
                if (File.Exists(fileName))      //文件存在就直接返回
                    return true;   
                IWorkbook workbook = null;
                if (fileName.IndexOf("xlsx") > -1)
                    workbook = new XSSFWorkbook();   //创建Excel工作簿  2007
                else if (fileName.IndexOf("xls") > -1)
                    workbook = new HSSFWorkbook();   //创建Excel工作簿  2003
                if (workbook == null)
                    return false;
                if (sheetName != null && sheetName.Length == 0)
                    sheetName = "Sheet1";
                ISheet sheet = workbook.CreateSheet(sheetName);//创建工作表
                IRow row = sheet.CreateRow(sheet.LastRowNum);//在工作表中添加一行用来存表头  
                ICell cell = null;
                int nIndex = 0;
                foreach (var it in colName)
                {
                    cell = row.CreateCell(nIndex++);
                    cell.SetCellValue(it);
                }
                FileStream fs = File.Create(fileName);
                workbook.Write(fs);
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    
        private Stream OpenClasspathResource(String fileName, bool bCreate=false)
        {
            if(!bCreate)
                return new FileStream(fileName, FileMode.Open, FileAccess.Read);
            else
                return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        private void WriteToFile(IWorkbook workbook, String fileName)
        {
            //Write the stream data of workbook to the root directory
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Write))
            {
                workbook.Write(file);
                file.Close();
            }
        }
        private HSSFWorkbook NPOIOpenExcel(string fileName)
        {
            Stream MyExcelStream = OpenClasspathResource(fileName);
            return new HSSFWorkbook(MyExcelStream);
        }

        /// <summary>  
        /// 将DataTable数据导入到excel中  
        /// </summary>  
        /// <param name="data">要导入的数据</param>  
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>  
        /// <param name="sheetName">要导入的excel的sheet的名称</param>  
        /// <returns>导入数据行数(包含列名那一行)</returns>  
        public int DataTableToExcel(DataTable dt, string sheetName, bool isColumnWritten,bool Append=true)
        {
            try
            {
                HSSFWorkbook Workbook = NPOIOpenExcel(FileName);//打开工作薄
                ISheet sheet = null;
                if (sheetName != null)
                    sheet = Workbook.GetSheet(sheetName);
                if (sheet == null)
                    sheet = Workbook.GetSheetAt(0);
                if (sheet == null)
                    return 0;
                int nStartRow = sheet.LastRowNum + 1;
                if (!Append)
                {
                    for (int i = 0; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if(row!=null)
                            sheet.RemoveRow(row);
                    }
                    nStartRow = 1;
                }

                //是否写入第一行
                if (isColumnWritten)
                {
                    IRow FirstRow= sheet.CreateRow(0);
                    int K = 0;
                    foreach (var it in dt.Columns)
                    {
                        FirstRow.CreateCell(K++).SetCellValue(it.ToString());
                    }
                }

                //不加第一行
                
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    IRow row = sheet.CreateRow(nStartRow+i);
                    for (int j = 0; j < dt.Columns.Count; j++)        //写一行的数据
                        row.CreateCell(j).SetCellValue(dr[j].ToString());
                }
                WriteToFile(Workbook, FileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"将DataTable写入到Excel文件出错:{ex.Message}");
            }
            return 1;
        }
        public int DataRowToExcel(DataRow dr, string sheetName)
        {
            try
            {
                HSSFWorkbook Workbook = NPOIOpenExcel(FileName);//打开工作薄
                ISheet sheet = null;
                if (sheetName != null && sheetName.Trim()=="")
                    sheet = Workbook.GetSheet(sheetName);
                if (sheet == null || sheetName.Trim() == "")
                    sheet = Workbook.GetSheetAt(0);
                if (sheet == null)
                    return 0;
                int nStartRow = sheet.LastRowNum + 1;
                IRow row = sheet.CreateRow(nStartRow);
                for (int i = 0; i < dr.ItemArray.Length; i++)        //写一行的数据
                    row.CreateCell(i).SetCellValue(dr[i].ToString());
                WriteToFile(Workbook, FileName);
            }
            catch(Exception ex)
            {
                throw new Exception($"将DataRow写入到Excel文件出错:{ex.Message}");
            }
            return 1;
        }
        public int ExcelToDataTable(ref DataTable dt,string sheetName)
        {
            try
            {
                if (dt == null)
                    return 0;
                dt.Columns.Clear();
                dt.Rows.Clear();
                dt.Clear(); //先清空表
                HSSFWorkbook Workbook = NPOIOpenExcel(FileName);//打开工作薄
                ISheet sheet = null;
                if (sheetName != null)
                    sheet = Workbook.GetSheet(sheetName);
                if (sheet == null)
                    sheet = Workbook.GetSheetAt(0);
                if (sheet == null)
                    return 0;
                IRow firstRow = sheet.GetRow(0);
                if (firstRow == null)
                    return 0;


                int nColCount = firstRow.LastCellNum;
                int nRowCount = sheet.LastRowNum + 1;

                for (int i = 0; i < nColCount; i++)
                {
                    string strValue = firstRow.GetCell(i).ToString();
                    if (strValue!=null && strValue.Trim()!="")
                    dt.Columns.Add(strValue);
                }

                for (int i = 1; i < nRowCount; i++)
                {
                    if (sheet.GetRow(i) == null || sheet.GetRow(i).GetCell(0)==null || sheet.GetRow(i).GetCell(0).ToString().Trim()=="")    //排除null
                        continue;
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < nColCount; j++)
                    {
                        string strField = firstRow.GetCell(j).ToString();
                        if (sheet.GetRow(i).GetCell(j) == null)    //排除null
                            continue;
                        dr[strField] = sheet.GetRow(i).GetCell(j).ToString();
                    }
                    dt.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"读取Excel文件到DataTable出错:{ex.Message}");
            }
            return 1;
        }
    }
}
