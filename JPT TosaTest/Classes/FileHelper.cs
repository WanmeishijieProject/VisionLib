using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes
{
    public class FileHelper
    {
        #region static method
        public static List<string> GetProfileList(string filePath)
        {
            var dir = filePath + "\\";
            if (Directory.Exists(dir))
            {
                List<string> list = new List<string>();

                DirectoryInfo info = new DirectoryInfo(dir);
                foreach (var file in info.GetFiles())
                    list.Add(System.IO.Path.GetFileNameWithoutExtension(file.FullName));
                return list;
            }
            else
                return null;
        }
        public static string GetCurFilePathString()
        {
            return System.Environment.CurrentDirectory + "\\";
        }
        public static void DeleteFile(string strFileFullPathName)
        {
            if (File.Exists(strFileFullPathName))
                File.Delete(strFileFullPathName);
        }
        public static void DeleteAllFileInDirectory(string filePath)
        {
            var dir = filePath + "\\";
            if (Directory.Exists(dir))
            {   
                DirectoryInfo info = new DirectoryInfo(dir);
                foreach (var file in info.GetFiles())
                    File.Delete(file.FullName);
            }
        }
        #endregion

        #region Work Directory
        private string _workDirectory = "";
        public FileHelper() { }
        public FileHelper(string strWorkDirectory)
        {
            if (Directory.Exists(strWorkDirectory))
                _workDirectory = strWorkDirectory + "\\";
            else
            {
                throw new Exception(string.Format("{0} is not exist", strWorkDirectory));
            }
        }
        public bool SetWorkDirectory(string strPath)
        {
            if (Directory.Exists(strPath))
            {
                _workDirectory = strPath + "\\";
                return true;
            }
            return false;
        }
        public void DeleteFileInDirectory(string strFileName)
        {
            if (File.Exists(_workDirectory + strFileName))
                File.Delete(_workDirectory + strFileName);
        }
        public List<string> GetWorkDictoryProfileList(string[] expNameListLookingFor)
        {
            if (Directory.Exists(_workDirectory))
            {
                List<string> list = new List<string>();
                DirectoryInfo info = new DirectoryInfo(_workDirectory);
                foreach (var file in info.GetFiles())
                {
                    if (expNameListLookingFor != null)
                    {
                        foreach (var ext in expNameListLookingFor)
                        {
                            if (file.Extension.Contains(ext))
                                list.Add(System.IO.Path.GetFileNameWithoutExtension(file.FullName));
                        }
                    }
                }
                return list;
            }
            else
                return null;
        }
        #endregion

    }
}
