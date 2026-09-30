using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTcpMonitoringApp.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _adminName = "기본관리자";

        [RelayCommand]
        private void SaveSettings()
        {
            // 💡 우체국(WeakReferenceMessenger)을 통해 편지를 전국에 방송(Broadcast)합니다!
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(AdminName));
        }
    }
}
