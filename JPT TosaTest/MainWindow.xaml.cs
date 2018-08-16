using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JPT_TosaTest.ViewModel;

namespace JPT_TosaTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();  
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var Vm = DataContext as MainViewModel;
            Vm.WindowLoadedCommand.Execute(null);

            //初始化站位信息栏
            int i = 0;
            StationInfoGrid.ShowGridLines = true;
            foreach (var it in Config.ConfigMgr.Instance.SoftwareCfgMgr.WorkFlowConfigs)
            {
                ColumnDefinition cd = new ColumnDefinition() { Width = new GridLength(1,GridUnitType.Star)};
                StationInfoGrid.ColumnDefinitions.Add(cd);
                TextBlock tb = new TextBlock();
                Binding bind = new Binding($"StationInfoCollection[{i}]");
                tb.SetBinding(TextBlock.TextProperty, bind);
                StationInfoGrid.Children.Add(tb);
                StationInfoGrid.Children[i].SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Center);
                StationInfoGrid.Children[i].SetValue(Grid.MarginProperty, new Thickness(10,0,0,0));
                Grid.SetColumn(tb,i++);                
            }

            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
        }
    }
}