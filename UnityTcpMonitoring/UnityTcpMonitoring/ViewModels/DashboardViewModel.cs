using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using UnityTcpMonitoring.Services;
using UnityTcpMonitoring.Views;
using UnityTcpMonitoringShared.Models;

namespace UnityTcpMonitoring.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private Machine? _selectedMachine;

        [ObservableProperty]
        private LineViewModel? _selectedLine;

        [ObservableProperty]
        private string _currentAdmin = "기본관리자";

        public ObservableCollection<Machine> MachineList { get; } = new();
        public ObservableCollection<LineViewModel> LineList { get; } = new();

        private readonly NetworkService _network;

        public DashboardViewModel(NetworkService network)
        {
            _network = network;

            _network.MachineListReceived += RefreshMachines;
            _network.MachineListRefreshed += RefreshMachines;
            _network.LineListReceived += RefreshLines;
            _network.LineListRefreshed += RefreshLines;

            WeakReferenceMessenger.Default.Register<UserChangedMessage>(this, (_, message) =>
            {
                CurrentAdmin = message.Value;
            });
        }

        [RelayCommand]
        private async Task OpenAddMachine()
        {
            var popupViewModel = new AddMachineView();
            var popupWindow = new AddMachineWindow
            {
                DataContext = popupViewModel,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow
            };

            popupWindow.ShowDialog();

            if (!popupViewModel.IsSavedSuccess)
                return;

            await _network.AddMachineAsync(popupViewModel.MachineName);
        }

        [RelayCommand]
        private async Task RefreshMachines()
        {
            // 설비와 라인 정보가 함께 화면을 구성하므로 두 목록을 같이 갱신한다.
            await _network.RefreshMachineAsync();
            await _network.RefreshLineAsync();
        }

        [RelayCommand]
        private async Task ControlMachine(string commandType)
        {
            if (SelectedMachine == null)
                return;

            await _network.ControlMachineAsync(commandType, SelectedMachine.Id);
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

            string message = machines.Count == 1
                ? $"[{machines[0].Name}] 설비를 삭제하시겠습니까?"
                : $"선택한 {machines.Count}개의 설비를 삭제하시겠습니까?";

            var result = MessageBox.Show(message, "삭제", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;

            foreach (var machine in machines)
                await _network.RemoveMachineAsync(machine.Id);

            SelectedMachine = null;
        }

        [RelayCommand]
        private async Task ImportExcel()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "설비 마스터 엑셀 파일 선택"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            byte[] fileBytes = File.ReadAllBytes(openFileDialog.FileName);
            await _network.ImportExcelAsync(fileBytes);
        }

        private void RefreshMachines(List<Machine> machines)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MachineList.Clear();

                foreach (var machine in machines)
                    MachineList.Add(machine);

                RebuildLineMachines();
            });
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
            await _network.ControlLineAsync(commandType, line.Id);
        }

        private void RebuildLineMachines()
        {
            foreach (var line in LineList)
            {
                var machines = MachineList.Where(x => x.LineId == line.Id);
                line.SetMachines(machines);
            }
        }
    }
}
