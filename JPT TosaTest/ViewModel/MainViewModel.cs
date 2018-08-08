using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using JPT_TosaTest.UserCtrl;
using System.Threading;

namespace JPT_TosaTest.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private int _viewIndex = 1;
        private int _errorCount = 0;
        private bool _boolShowInfoListBox = false;
        private AutoResetEvent OpenedEvent = new AutoResetEvent(true);
       


        public MainViewModel(IDataService dataService)
        {
            TestItemCollection = new ObservableCollection<TestItemModel>()
            {
                new TestItemModel(){ ItemName="Item1", PosX=19.876, PosY=22.987, ItemColor="Green"},
                new TestItemModel(){ ItemName="Item1", PosX=19.876, PosY=22.987, ItemColor="Green"},
                new TestItemModel(){ ItemName="Item1", PosX=19.876, PosY=22.987, ItemColor="Green"},
                new TestItemModel(){ ItemName="Item1", PosX=19.876, PosY=22.987, ItemColor="Green"},
            };
            ResultCollection = new ObservableCollection<ResultItem>()
            {
              new ResultItem(){ Index=1, HSG_X=1, HSG_Y=2, HSG_R=3, PLC_X=5, PLC_Y=6, PLC_R=7 }
            };
            SystemErrorMessageCollection = new ObservableCollection<MessageItem>();
            SystemErrorMessageCollection.CollectionChanged += SystemErrorMessageCollection_CollectionChanged;
        }
        private void SystemErrorMessageCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var colls = from item in SystemErrorMessageCollection where item.MsgType == EnumMessageType.Error select item;
            if (colls != null)
                ErrorCount = colls.Count();
        }

        #region Property
        public int ViewIndex
        {
            get { return _viewIndex; }
            set
            {
                if (_viewIndex != value)
                {
                    _viewIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<TestItemModel> TestItemCollection
        {
            get;
            set;
        }
        public ObservableCollection<ResultItem> ResultCollection
        {
            get;
            set;
        }
        public bool BoolShowInfoListBox
        {
            get { return _boolShowInfoListBox; }
            set
            {
                if (_boolShowInfoListBox != value)
                {
                    _boolShowInfoListBox = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int ErrorCount
        {
            get { return _errorCount; }
            set
            {
                if (_errorCount != value)
                {
                    _errorCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<MessageItem> SystemErrorMessageCollection
        {
            get;
            set;
        }
        #endregion



        #region Command
        public RelayCommand BtnHomeCommand
        {
            get { return new RelayCommand(() => ViewIndex = 1); }
        }
        public RelayCommand BtnSettingCommand
        {
            get { return new RelayCommand(() => ViewIndex = 2); }
        }
        public RelayCommand ShowInfoListCommand
        {
            get
            {
                return new RelayCommand(() =>
                      {
                          SystemErrorMessageCollection.Add(new MessageItem() { MsgType = EnumMessageType.Error, StrMsg = "Error message" });
                          ViewIndex = 3;
                      });
            }
        }
        public RelayCommand ClearMessageCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SystemErrorMessageCollection.Clear();
                });
            }
        }
        public RelayCommand AddPreSetCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    TestItemCollection.Add(new TestItemModel() { ItemName = "Item1", PosX = 19.876, PosY = 22.987, ItemColor = "Green" });
                });
            }
        }
        public RelayCommand<int> RemoveSelectedItemCommand
        {
            get
            {
                return new RelayCommand<int>(nIndex =>
                {
                    if (nIndex >= 0)
                        TestItemCollection.RemoveAt(nIndex);
                });
            }
        }
        public RelayCommand BtnTeachCommand
        {
            get
            {
                return new RelayCommand(() =>
               {

                   if (OpenedEvent.WaitOne(200))
                   {
                       Window_TeachBox dlg = new Window_TeachBox(ref OpenedEvent);
                       dlg.Show();
                   }
               });
            }
        }

        public RelayCommand BtnCameraCommand
        {
            get { return new RelayCommand(() => ViewIndex = 4); }
        }

        public RelayCommand BtnLogInCommand
        {
            get
            {
                return new RelayCommand(() => ViewIndex = 5);
            }
        }
        #endregion
    }
}