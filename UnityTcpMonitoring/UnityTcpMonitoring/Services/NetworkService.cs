using UnityTcpMonitoringServer;
using UnityTcpMonitoringShared.Models;

namespace UnityTcpMonitoring.Services
{
    public partial class NetworkService
    {
        private PacketSender? _sender;
        private readonly ServerConnection _connector;
        private CancellationTokenSource? _networkCts;

        public event Action<List<Machine>>? MachineListReceived;
        public event Action<List<Machine>>? MachineListRefreshed;
        public event Action<List<Line>>? LineListReceived;
        public event Action<List<Line>>? LineListRefreshed;

        public NetworkService()
        {
            var handler = new PacketHandler();

            handler.MachineListReceived += machines => MachineListReceived?.Invoke(machines);
            handler.MachineListRefreshed += machines => MachineListRefreshed?.Invoke(machines);
            handler.LineListReceived += lines => LineListReceived?.Invoke(lines);
            handler.LineListRefreshed += lines => LineListRefreshed?.Invoke(lines);

            _connector = new ServerConnection(handler);
            _connector.Connected += stream => _sender = new PacketSender(stream);

            StartLoop();
        }

        private PacketSender Sender =>
            _sender ?? throw new InvalidOperationException("중앙 서버에 연결되지 않았습니다.");

        public Task InitLineAsync()
        {
            return Sender.SendAsync(new { Command = Commands.InitLine });
        }

        public Task ControlLineAsync(string commandType, int lineId)
        {
            return Sender.SendAsync(new
            {
                Command = Commands.ControlLine,
                Id = lineId,
                Action = commandType
            });
        }

        public Task RefreshLineAsync()
        {
            return Sender.SendAsync(new { Command = Commands.RefreshLine });
        }

        public Task RefreshMachineAsync()
        {
            return Sender.SendAsync(new { Command = Commands.RefreshMachine });
        }

        public Task RemoveMachineAsync(int id)
        {
            return Sender.SendAsync(new
            {
                Command = Commands.RemoveMachine,
                Id = id
            });
        }

        public Task AddMachineAsync(string machineName)
        {
            return Sender.SendAsync(new
            {
                Command = Commands.AddMachine,
                Name = machineName
            });
        }

        public Task ControlMachineAsync(string commandType, int id)
        {
            return Sender.SendAsync(new
            {
                Command = Commands.ControlMachine,
                Id = id,
                Action = commandType
            });
        }

        public Task ImportExcelAsync(byte[] fileBytes)
        {
            return Sender.SendAsync(new
            {
                Command = Commands.ImportExcel,
                FileData = Convert.ToBase64String(fileBytes)
            });
        }

        private void StartLoop()
        {
            _networkCts = new CancellationTokenSource();
            _ = Task.Run(() => _connector.ConnectAsync(_networkCts.Token));
        }
    }
}
