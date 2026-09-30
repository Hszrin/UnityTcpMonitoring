using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnityTcpMonitoring.Shared.Models;
using UnityTcpMonitoringServer.Repositories;
using System.Collections.Generic; // List 사용을 위해 추가
using System.Threading.Tasks;
using System.Windows;

namespace UnityTcpMonitoringApp.ViewModels
{
    public partial class AddMachineView : ObservableObject
    {
        [ObservableProperty]
        private string _machineName = string.Empty;

        // 🌟 1. 사용자가 드롭다운에서 '최종 선택한' 품목을 담을 변수
        [ObservableProperty]
        private string _selectedProductType = string.Empty;

        // 🌟 2. 드롭다운에 보여줄 '정해진 품목 리스트' (여기서 품목을 마음껏 추가/수정 하시면 됩니다!)
        public List<string> ProductTypeList { get; } = new()
        {
            "스마트폰 액정",
            "스마트폰 배터리",
            "반도체 칩",
            "기본 케이스",
            "카메라 모듈"
        };

        private readonly IMachineRepository _machineRepository;
        public bool IsSavedSuccess { get; private set; } = false;

        public AddMachineView()
        {
            _machineRepository = new MachineRepository();

            MachineName = string.Empty;
            // 초기 선택값을 리스트의 첫 번째 항목으로 지정을 해두면 비어있지 않아 안전합니다.
            if (ProductTypeList.Count > 0)
            {
                SelectedProductType = ProductTypeList[0];
            }
        }

        [RelayCommand]
        private async Task AddMachine(Window currentWindow)
        {
            if (string.IsNullOrWhiteSpace(MachineName)) return;
            Console.WriteLine(MachineName);

            try
            {
                var newMachine = new Machine
                {
                    Name = MachineName,
                    Status = "정지",
                };

                await _machineRepository.AddMachineAsync(newMachine);

                IsSavedSuccess = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"기계 등록 실패: {ex.Message}");
            }
            currentWindow?.Close();
        }
    }
}