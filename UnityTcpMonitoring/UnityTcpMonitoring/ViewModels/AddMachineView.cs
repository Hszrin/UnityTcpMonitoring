using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace UnityTcpMonitoring.ViewModels
{
    public partial class AddMachineView : ObservableObject
    {
        [ObservableProperty]
        private string _machineName = string.Empty;

        public bool IsSavedSuccess { get; private set; }

        [RelayCommand]
        private void AddMachine(Window? currentWindow)
        {
            if (string.IsNullOrWhiteSpace(MachineName))
            {
                MessageBox.Show("설비 이름을 입력해 주세요.");
                return;
            }

            // 실제 DB 등록은 서버가 담당하고, 팝업은 입력 결과만 반환한다.
            IsSavedSuccess = true;
            currentWindow?.Close();
        }
    }
}
