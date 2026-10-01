using UnityTcpMonitoringShared.Models;

namespace UnityTcpMonitoringServer.Repositories
{
    public interface IMachineRepository
    {
        Task<IEnumerable<Machine>> GetAllMachinesAsync();
        Task AddMachineAsync(Machine machine);
        Task AddRangeMachineAsync(List<Machine> machines);
        Task SaveMachineAsync(Machine machine);
        Task RemoveMachineAsync(int machineId);
        Task UpdateMachineAsync(
            int machineId,
            string newStatus,
            int productionCount,
            string lastProductionTime);
    }
}
