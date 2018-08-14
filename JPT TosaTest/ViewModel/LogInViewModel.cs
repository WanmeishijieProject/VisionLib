using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Config;
using JPT_TosaTest.Config.UserManager;
using JPT_TosaTest.UserCtrl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.ViewModel
{
    public class LogInViewModel : ViewModelBase
    {
        public LogInViewModel()
        {
            //User config
            UserModelCollection = new ObservableCollection<UserModel>();
            foreach (var it in ConfigMgr.Instance.UserCfgMgr.Users)
            {
                UserModelCollection.Add(it);
            }
        }


        private int _level = 0;
        private string _currentStrUserName = "";
        private bool _showSerUserPsw = false;
        #region Properties

        public ObservableCollection<UserModel> UserModelCollection { get; set; }

        //当前用户等级
        public int Level
        {
            set
            {
                if (_level != value)
                {
                    _level = value;
                    RaisePropertyChanged();
                }
            }
            get { return _level; }
        }

        //当前用户名称
        public string CurrentStrUserName
        {
            set
            {
                if (_currentStrUserName != value)
                {
                    _currentStrUserName = value;
                    RaisePropertyChanged();
                }
            }
            get { return _currentStrUserName; }
        }

        //显示修改密码框
        public bool ShowSerUserPsw
        {
            set
            {
                if (_showSerUserPsw != value)
                {
                    _showSerUserPsw = value;
                    RaisePropertyChanged();
                }
            }
            get { return _showSerUserPsw; }
        }
        #endregion


        #region Commands
        public RelayCommand LogOutCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Level = 0;
                    CurrentStrUserName = "Operator";
                });
            }
        }
        public RelayCommand<Tuple<string,string>> LogInCommand
        {
            get
            {
                return new RelayCommand<Tuple<string,string>>(tuple =>
                {
                    if (tuple == null)
                        return;
                    string UserEdit = tuple.Item1;
                    string PswdEdit = tuple.Item2;
                    foreach (var it in ConfigMgr.Instance.UserCfgMgr.Users)
                    {
                        if (!string.IsNullOrEmpty(UserEdit) && !string.IsNullOrEmpty(PswdEdit))
                        {
                            if (it.User == UserEdit && it.Password == PswdEdit)
                            {
                                Level = it.Level;
                                CurrentStrUserName = it.User;
                                break;
                            }
                        }
                    }
                });
            }
        }
        public RelayCommand ShowModifyPsdCommand
        {
            get
            {
                return new RelayCommand(() => {
                    ShowSerUserPsw = !ShowSerUserPsw;
                });
            }
        }
        public RelayCommand<UserModel> SaveUserCfgCommand
        {
            get
            {
                return new RelayCommand<UserModel>(user =>
                {
                    try
                    {
                        if (user.Password.Trim() == "")
                        {
                            UC_MessageBox.ShowMsgBox("密码不能为空", "提示");
                            return;
                        }
                        ConfigMgr.Instance.SaveConfig(EnumConfigType.UserCfg, UserModelCollection.ToArray());
                        UC_MessageBox.ShowMsgBox("修改密码成功", "成功");
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox(ex.Message);
                    }
                });
            }

        }
        #endregion

    }
}
