using System.Windows;
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
        }
    }
}