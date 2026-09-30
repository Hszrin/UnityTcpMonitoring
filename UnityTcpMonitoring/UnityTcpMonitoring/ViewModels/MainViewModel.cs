using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnityTcpMonitoringApp.Services;

namespace UnityTcpMonitoringApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // 💡 현재 화면 중앙에 렌더링할 뷰모델을 담는 주머니
        [ObservableProperty]
        private ObservableObject _currentView;

        // 서브 뷰모델 주머니 타입을 부모인 ObservableObject로 통일!
        private readonly ObservableObject _dashboardVM;
        private readonly ObservableObject _settingVM;

        private readonly NetworkService _network = new();

        public MainViewModel()
        {
            // 다른 서버 통신이나 복잡한 초기화 코드는 전부 지우고 딱 이 3줄만 남깁니다!
            _dashboardVM = new DashboardViewModel(_network);
            _settingVM = new SettingViewModel();

            _currentView = _dashboardVM;
        }

        [RelayCommand]
        private void MoveToDashboard()
        {
            CurrentView = _dashboardVM;
        }

        [RelayCommand]
        private void MoveToSetting()
        {
            CurrentView = _settingVM;
        }
    }
}