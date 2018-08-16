using JPT_TosaTest.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JPT_TosaTest.UserCtrl
{
    /// <summary>
    /// UC_HomeView.xaml 的交互逻辑
    /// </summary>
    public partial class UC_HomeView : UserControl
    {
        private AutoResetEvent SyncEvent = null;
        private bool bFirstLoaded = false;
        private object Lock = null;
        public UC_HomeView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            HalconVision.Instance.GetSyncSp(out SyncEvent, out Lock, 0);
            SyncEvent.WaitOne(100);
            LoadDelay(2000);
            bFirstLoaded = true;
        }
        private void SetAttachCamWindow( bool bAttach = true)
        {
            if (bAttach)
            {
                HalconVision.Instance.AttachCamWIndow(0, "Cam1", Cam1.HalconWindow);
                HalconVision.Instance.AttachCamWIndow(1, "Cam2", Cam2.HalconWindow);
            }
            else
            {
                HalconVision.Instance.DetachCamWindow(0, "Cam1");
                HalconVision.Instance.DetachCamWindow(1, "Cam2");
            }
        }
        private async void LoadDelay(int ms)
        {
            await Task.Run(() => {
                if (bFirstLoaded)
                {
                    Task.Delay(ms).Wait();
                    bFirstLoaded = false;
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() => SetAttachCamWindow(true));
            });
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetAttachCamWindow(Convert.ToBoolean(e.NewValue));
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Lock != null)
            {
                lock (Lock)
                {
                    if (SyncEvent != null)
                        SyncEvent.Set();
                }
            }

        }
    }
}
