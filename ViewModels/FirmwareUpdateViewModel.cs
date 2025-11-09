using Prism.Mvvm;

namespace CANMonitor.ViewModels
{
    public class FirmwareUpdateViewModel : BindableBase
    {
        private string _title = "Firmware Update";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}