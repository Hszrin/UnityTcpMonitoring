using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace UnityTcpMonitoring.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _adminName = "기본관리자";

        [RelayCommand]
        private void SaveSettings()
        {
            // 설정 화면과 대시보드를 직접 참조하지 않도록 메시지로 변경 사항을 전달한다.
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(AdminName));
        }
    }
}
