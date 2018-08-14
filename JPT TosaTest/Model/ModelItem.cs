using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JPT_TosaTest.Classes;
using JPT_TosaTest.UserCtrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Models
{
    public class ModelItem : RoiModelBase
    {

        public override RelayCommand<RoiModelBase> OperateDelete
        {
            get
            {
                return new RelayCommand<RoiModelBase>(item => {
                    var model= item as ModelItem;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(FileHelper.GetCurFilePathString());
                    sb.Append("VisionData\\Model\\");
                    sb.Append(item.StrFullName);
                    int nCamID = Convert.ToInt16(item.StrFullName.Substring(3, 1));
                    if (UC_MessageBox.ShowMsgBox(string.Format("确定要删除{0}吗?", item.StrName)) == System.Windows.MessageBoxResult.Yes)
                    {
                        //三个文件同时删除
                        FileHelper.DeleteFile(sb.ToString()+".shm");
                        FileHelper.DeleteFile(sb.ToString() + ".reg");
                        FileHelper.DeleteFile(sb.ToString() + ".tup");
                        Messenger.Default.Send<int>(nCamID, "UpdateTemplateFiles");
                    }   
                });
            }
        }
    }
}
