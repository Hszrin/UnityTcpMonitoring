using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnityTcpMonitoringShared.Models;
using UnityTcpMonitoringApp.Services;
using UnityTcpMonitoringApp.Views;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
namespace UnityTcpMonitoringApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private Machine _selectedMachine; 
        public ObservableCollection<Machine> MachineList { get; set; } = new(); 
        public ObservableCollection<LineViewModel> LineList { get; } = new();

        private readonly NetworkService _network;
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _newMachineName = string.Empty; // 💡 사용자가 입력할 기계 이름을 담을 변수
        public DashboardViewModel(NetworkService network)
        {
            _network = network;

            _network.MachineListReceived += RefreshMachines;
            _network.MachineListRefreshed += RefreshMachines;

            _network.LineListReceived += RefreshLines;
            _network.LineListRefreshed += RefreshLines;
        }
        [RelayCommand]
        private async Task OpenAddMachine() // 🌟 await를 쓰기 위해 async Task로 변경해 줍니다!
        {
            Debug.WriteLine("설비 등록 창 열기 버튼 클릭됨!");

            var popupWindow = new AddMachineWindow();
            var popupViewModel = new AddMachineView();

            popupWindow.DataContext = popupViewModel;
            popupWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // 🌟 부모 창을 정확히 지정해 주어야 CenterOwner가 작동하고, 화면이 이쁘게 묶입니다.
            if (Application.Current.MainWindow != null)
            {
                popupWindow.Owner = Application.Current.MainWindow;
            }

            // 4. 화면에 팝업창을 띄우고 유저가 창을 닫을 때까지 여기서 대기합니다.
            popupWindow.ShowDialog();

            // 🌟 5. 마법의 구간: 창이 닫힌 후 일로 내려옵니다! 
            // 팝업창 뷰모델 안에서 저장이 진짜 성공(true)했는지 확인합니다.
            if (popupViewModel.IsSavedSuccess)
            {
                // 팝업창 입력 필드에 적힌 새 기계 이름을 가져옵니다.
                string machineName = popupViewModel.MachineName;
                Console.WriteLine(machineName);

                await _network.AddMachineAsync(machineName, SelectedMachine.Id);
            }
        }
        [RelayCommand]
        private async Task ControlMachine(string commandType)
        {
            // 상단 그리드에서 선택된 기계가 없거나
            if (SelectedMachine == null) return;

            await _network.ControlMachineAsync(commandType, SelectedMachine.Id);
        }
        private void RefreshMachines(List<Machine> machines)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MachineList.Clear();

                foreach (var machine in machines)
                {
                    MachineList.Add(machine);
                }

                RebuildLineMachines();
            });
        }
        [RelayCommand]
        private async Task RemoveMachine(IList selectedItems)
        {
            var machines = selectedItems.Cast<Machine>().ToList();

            if (machines.Count == 0)
            {
                MessageBox.Show("삭제할 설비를 선택해 주세요.");
                return;
            }

            string message;

            if (machines.Count == 1)
            {
                message = $"[{machines[0].Name}] 설비를 삭제하시겠습니까?";
            }
            else
            {
                message = $"선택한 {machines.Count}개의 설비를 삭제하시겠습니까?";
            }

            var result = MessageBox.Show(
                message,
                "삭제",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            foreach (var machine in machines)
            {
                await _network.RemoveMachineAsync(machine.Id);
            }

            SelectedMachine = null;
        }
        private void RefreshLines(List<Line> lines)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                LineList.Clear();

                foreach (var line in lines)
                {
                    var viewModel = new LineViewModel(line);

                    viewModel.ControlRequested += OnLineControlRequested;

                    LineList.Add(viewModel);
                }

                RebuildLineMachines();
            });
        }
        private async void OnLineControlRequested(LineViewModel line, string commandType)
        {
            await _network.ControlLineAsync(
                commandType,
                line.Id);
        }
        private void RebuildLineMachines()
        {
            foreach (var line in LineList)
            {
                var machines = MachineList
                    .Where(x => x.LineId == line.Id);

                line.SetMachines(machines);
            }
        }
        [RelayCommand]
        private async Task ImportExcel()
        {
            // 1. 윈도우 파일 선택창 열기
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "설비 마스터 엑셀 파일 선택"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // 2. 엑셀 파일을 바이트 배열로 읽기
                byte[] fileBytes = File.ReadAllBytes(openFileDialog.FileName);
                await _network.ImportExcelAsync(fileBytes);
            }
        }
    }
}