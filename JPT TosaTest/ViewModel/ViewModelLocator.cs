using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using JPT_TosaTest.Model;

namespace JPT_TosaTest.ViewModel
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingVM>();
            SimpleIoc.Default.Register<LogInViewModel>();
            SimpleIoc.Default.Register<CamDebugViewModel>();
            SimpleIoc.Default.Register<TeachBoxViewModel>();
            SimpleIoc.Default.Register<MonitorViewModel>();
            SimpleIoc.Default.Register<AligmentViewModel>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public SettingVM SettingViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingVM>();
            }
        }
        public LogInViewModel LogInVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LogInViewModel>();
            }
        }
        public CamDebugViewModel CamDebugVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CamDebugViewModel>();
            }
        }
        public TeachBoxViewModel TeachBoxVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TeachBoxViewModel>();
            }
        }
        public MonitorViewModel MonitorVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MonitorViewModel>();
            }
        }
        public AligmentViewModel AligmentVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AligmentViewModel>();
            }
        }

        public static void Cleanup()
        {

        }
    }
}