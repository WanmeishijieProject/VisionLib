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
    public class RoiItem : RoiModelBase
    {
        //public override  RelayCommand<RoiModelBase> OperateAdd
        //{
        //    get
        //    {
        //        return new RelayCommand<RoiModelBase>(item =>
        //        {
        //            var model = item as RoiItem;
        //            Vision.Vision.Instance.DrawRoi(model.Index);                  
        //        });
        //    }
        //}

        public override RelayCommand<RoiModelBase> OperateDelete => new RelayCommand<RoiModelBase>(item =>
        {
            var model = item as RoiItem;
            StringBuilder sb = new StringBuilder();
            sb.Append(FileHelper.GetCurFilePathString());
            sb.Append("VisionData\\Roi\\");
            sb.Append(item.StrFullName);
            int nCamID =Convert.ToInt16( item.StrFullName.Substring(3, 1));
            if (UC_MessageBox.ShowMsgBox(string.Format("确定要删除{0}吗?", item.StrName)) == System.Windows.MessageBoxResult.Yes)
            {
                FileHelper.DeleteFile(sb.ToString()+".reg");
                FileHelper.DeleteFile(sb.ToString() + ".tup");
                Messenger.Default.Send<int>(nCamID, "UpdateRoiFiles");
            }
        });

    }
}
