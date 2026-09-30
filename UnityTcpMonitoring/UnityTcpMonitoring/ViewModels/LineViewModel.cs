using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnityTcpMonitoring.Shared.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnityTcpMonitoringApp.ViewModels
{
    public partial class LineViewModel : ObservableObject
    {
        public Line Line { get; }
        public ObservableCollection<Machine> Machines { get; } = new();

        public int Id => Line.Id;
        public string Name => Line.Name;
        public double TargetProductionRate => Line.TargetProductionRate;
        public string Status => Line.Status;

        public double ProductionCount
        {
            get
            {
                if (Machines.Count == 0)
                    return 0;

                return Machines.Min(x => x.ProductionCount);
            }
        }

        public double AchievementRate
        {
            get
            {
                if (TargetProductionRate <= 0)
                    return 0;

                return ProductionCount / TargetProductionRate * 100;
            }
        }

        public event Action<LineViewModel, string>? ControlRequested;

        public LineViewModel(Line line)
        {
            Line = line;
        }

        [RelayCommand]
        private void ControlLine(string commandType)
        {
            ControlRequested?.Invoke(this, commandType);
        }

        public void SetMachines(IEnumerable<Machine> machines)
        {
            Machines.Clear();

            foreach (var machine in machines)
            {
                Machines.Add(machine);
            }

            Refresh();
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(ProductionCount));
            OnPropertyChanged(nameof(AchievementRate));
            OnPropertyChanged(nameof(Status));
        }
    }
}