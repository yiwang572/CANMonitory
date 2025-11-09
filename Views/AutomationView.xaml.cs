using CANMonitor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CANMonitor.Views
{
    /// <summary>
    /// AutomationView.xaml 的交互逻辑
    /// </summary>
    public partial class AutomationView : Window
    {
        public AutomationView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 窗口加载时更新状态栏
            UpdateStatusBar("窗口加载完成");
        }

        private void OnTemplateSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 当模板选择改变时，可以在这里添加额外的处理逻辑
            var viewModel = (AutomationViewModel)DataContext;
            if (viewModel != null && viewModel.SelectedTemplate != null)
            {
                UpdateStatusBar($"已选择模板: {viewModel.SelectedTemplate.TemplateName}");
            }
        }

        private void UpdateStatusBar(string statusText)
        {
            if (StatusBarText != null)
            {
                StatusBarText.Text = statusText;
            }
        }

        // 处理步骤列表的索引显示
        private void StepsListBox_Loaded(object sender, RoutedEventArgs e)
        {
            // 在实际应用中，可以在这里设置ListBox的样式和行为
        }

        // 处理步骤属性面板的动态更新
        private void SelectedStepChanged(object sender, SelectionChangedEventArgs e)
        {
            // 当选中步骤改变时，可以动态更新参数设置面板
            UpdateParametersPanel();
        }

        private void UpdateParametersPanel()
        {
            // 在实际应用中，这里应该根据选中步骤的类型动态生成参数设置控件
            // 示例实现将在ViewModel中处理
        }
    }
}