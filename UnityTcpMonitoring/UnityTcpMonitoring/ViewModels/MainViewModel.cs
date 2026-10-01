using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnityTcpMonitoring.Services;

namespace UnityTcpMonitoring.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject _currentView;

        private readonly ObservableObject _dashboardViewModel;
        private readonly ObservableObject _settingViewModel;
        private readonly NetworkService _network = new();

        public MainViewModel()
        {
            _dashboardViewModel = new DashboardViewModel(_network);
            _settingViewModel = new SettingViewModel();
            _currentView = _dashboardViewModel;
        }

        [RelayCommand]
        private void MoveToDashboard()
        {
            CurrentView = _dashboardViewModel;
        }

        [RelayCommand]
        private void MoveToSetting()
        {
            CurrentView = _settingViewModel;
        }
    }
}
