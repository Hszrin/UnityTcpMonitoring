using System.Text.Json;
using UnityTcpMonitoringServer.Repositories;

namespace UnityTcpMonitoringServer.Services
{
    internal class UnityPacketHandler
    {
        private readonly MachineRepository _machineRepository;
        private readonly ClientManager _clientManager;

        public UnityPacketHandler(
            MachineRepository machineRepository,
            ClientManager clientManager)
        {
            _machineRepository = machineRepository;
            _clientManager = clientManager;
        }

        public async Task ProcessAsync(PacketHeader packet, string json)
        {
            switch (packet.Command)
            {
                case Commands.Heartbeat:
                {
                    var heartbeat = JsonSerializer.Deserialize<HeartbeatData>(json);
                    if (heartbeat != null)
                        await HandleHeartbeatAsync(heartbeat);
                    break;
                }
                case Commands.ProductionUpdate:
                {
                    var production = JsonSerializer.Deserialize<MachineStatusDto>(json);
                    if (production != null)
                        await HandleProductionUpdateAsync(production);
                    break;
                }
            }
        }

        private async Task HandleProductionUpdateAsync(MachineStatusDto data)
        {
            await _machineRepository.UpdateMachineAsync(
                data.Id,
                data.Status ?? "정지",
                data.ProductionCount,
                data.LastProductionTime ?? string.Empty);

            await _clientManager.BroadcastRefreshAsync();
        }

        private async Task HandleHeartbeatAsync(HeartbeatData data)
        {
            if (data.Machines == null)
                return;

            foreach (var machine in data.Machines)
            {
                await _machineRepository.UpdateMachineAsync(
                    machine.Id,
                    machine.Status ?? "정지",
                    machine.ProductionCount,
                    machine.LastProductionTime ?? string.Empty);
            }

            await _clientManager.BroadcastRefreshAsync();
        }
    }

    public class HeartbeatData
    {
        public List<MachineStatusDto>? Machines { get; set; }
    }

    public class MachineStatusDto
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public int ProductionCount { get; set; }
        public string? LastProductionTime { get; set; }
    }
}
