using OfficeOpenXml;
using System.Text.Json;
using UnityTcpMonitoringServer.Repositories;
using UnityTcpMonitoringShared.Models;

namespace UnityTcpMonitoringServer.Services
{
    internal class WPFPacketHandler
    {
        private readonly MachineRepository _machineRepository;
        private readonly LineRepository _lineRepository;
        private readonly ClientManager _clientManager;

        public WPFPacketHandler(
            MachineRepository machineRepository,
            LineRepository lineRepository,
            ClientManager clientManager)
        {
            _machineRepository = machineRepository;
            _lineRepository = lineRepository;
            _clientManager = clientManager;
        }

        public async Task ProcessAsync(PacketHeader packet, string json)
        {
            var wpfPacket = JsonSerializer.Deserialize<WPFPacketDto>(json);
            if (wpfPacket == null)
                return;

            switch (packet.Command)
            {
                case Commands.AddMachine:
                    await HandleAddMachineAsync(wpfPacket);
                    break;
                case Commands.RemoveMachine:
                    await HandleRemoveMachineAsync(wpfPacket);
                    break;
                case Commands.RefreshMachine:
                    await _clientManager.BroadcastRefreshAsync();
                    break;
                case Commands.ControlMachine:
                    await HandleControlMachineAsync(wpfPacket);
                    break;
                case Commands.ImportExcel:
                    await HandleImportExcelAsync(wpfPacket);
                    break;
                case Commands.ControlLine:
                    await HandleControlLineAsync(wpfPacket);
                    break;
            }
        }

        private async Task HandleAddMachineAsync(WPFPacketDto packet)
        {
            var newMachine = new Machine
            {
                Name = string.IsNullOrWhiteSpace(packet.Name) ? "미지정 설비" : packet.Name,
                Status = "정지"
            };

            await _machineRepository.AddMachineAsync(newMachine);
            await _clientManager.BroadcastRefreshAsync();
        }

        private async Task HandleRemoveMachineAsync(WPFPacketDto packet)
        {
            await _machineRepository.RemoveMachineAsync(packet.Id);
            await _clientManager.BroadcastRefreshAsync();
        }

        private async Task HandleControlMachineAsync(WPFPacketDto packet)
        {
            await _machineRepository.UpdateMachineStatus(
                packet.Id,
                packet.Action ?? string.Empty);

            await _clientManager.BroadcastRefreshAsync();
        }

        private async Task HandleControlLineAsync(WPFPacketDto packet)
        {
            string status = packet.Action ?? "정지";

            await _lineRepository.UpdateLineStatusAsync(packet.Id, status);
            await _clientManager.ControlLineToUnityAsync();
            await _clientManager.BroadcastRefreshAsync();
        }

        private async Task HandleImportExcelAsync(WPFPacketDto packet)
        {
            ExcelPackage.License.SetNonCommercialPersonal("김준원");

            if (string.IsNullOrWhiteSpace(packet.FileData))
                return;

            byte[] fileBytes = Convert.FromBase64String(packet.FileData);

            using var memoryStream = new MemoryStream(fileBytes);
            using var package = new ExcelPackage(memoryStream);

            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet.Dimension == null)
                return;

            var excelMachines = new List<Machine>();

            // 첫 행은 헤더로 사용하고, 2행부터 설비명과 LineId를 읽는다.
            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                var nameValue = worksheet.Cells[row, 1].Value;
                var lineValue = worksheet.Cells[row, 2].Value;

                if (nameValue == null)
                    continue;

                string machineName = nameValue.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(machineName))
                    continue;

                if (lineValue == null ||
                    !int.TryParse(lineValue.ToString(), out int lineId) ||
                    lineId <= 0)
                {
                    Console.WriteLine($"[Excel] {row}행 LineId가 올바르지 않습니다.");
                    continue;
                }

                excelMachines.Add(new Machine
                {
                    Name = machineName,
                    Status = "정지",
                    LineId = lineId,
                    ProductionCount = 0,
                    LastProductionTime = string.Empty
                });
            }

            if (excelMachines.Count == 0)
                return;

            var existingLines = await _lineRepository.GetAllLinesAsync();
            var existingLineIds = existingLines.Select(x => x.Id).ToHashSet();
            var requiredLineIds = excelMachines.Select(x => x.LineId).Distinct();

            // Excel에만 존재하는 라인은 설비 등록 전에 먼저 생성하여 FK 관계를 보장한다.
            foreach (int lineId in requiredLineIds)
            {
                if (existingLineIds.Contains(lineId))
                    continue;

                await _lineRepository.AddLineAsync(new Line
                {
                    Id = lineId,
                    Name = $"{lineId}라인",
                    TargetProductionRate = 100
                });
            }

            await _machineRepository.AddRangeMachineAsync(excelMachines);
            await _clientManager.BroadcastRefreshAsync();
        }
    }

    public class WPFPacketDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Action { get; set; }
        public string? FileData { get; set; }
    }
}
