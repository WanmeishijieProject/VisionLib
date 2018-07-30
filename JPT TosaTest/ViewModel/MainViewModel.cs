using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Model;
using System.Collections.ObjectModel;
using System.Linq;
namespace JPT_TosaTest.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private int _viewIndex = 1;
        public int _errorCount = 0;
        public bool _boolShowInfoListBox = false;
        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }
            set
            {
                Set(ref _welcomeTitle, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = item.Title;
                });

            TestItem = new ObservableCollection<TestItemModel>()
            {
                new TestItemModel(){ ItemName="Item1", ItemValue="Value1"},
                new TestItemModel(){ ItemName="Item2", ItemValue="Value2"},
                new TestItemModel(){ ItemName="Item3", ItemValue="Value3"}
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
        public ObservableCollection<TestItemModel> TestItem
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
            get { return new RelayCommand(() =>
            {
                SystemErrorMessageCollection.Add(new MessageItem() { MsgType = EnumMessageType.Error, StrMsg = "Error message" });
                BoolShowInfoListBox = !BoolShowInfoListBox;
            }); }
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

        #endregion
    }
}