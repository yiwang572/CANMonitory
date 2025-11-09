using Prism.Mvvm;

namespace CANMonitor.ViewModels
{
    public class MessageSendViewModel : BindableBase
    {
        private string _title = "Message Send";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}