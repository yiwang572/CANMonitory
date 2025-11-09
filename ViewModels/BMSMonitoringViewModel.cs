using Prism.Mvvm;

namespace CANMonitor.ViewModels
{
    public class CANMonitoringViewModel : BindableBase
    {
        private string _title = "BMS Monitoring";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}