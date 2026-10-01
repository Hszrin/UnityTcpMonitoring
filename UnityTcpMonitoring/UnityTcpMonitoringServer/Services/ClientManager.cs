using System.Net.Sockets;
using UnityTcpMonitoringServer.Repositories;

namespace UnityTcpMonitoringServer.Services
{
    internal class ClientManager
    {
        public TcpClient? UnityClient { get; private set; }
        public TcpClient? WpfClient { get; private set; }

        private readonly PacketSender _packetSender = new();
        private readonly MachineRepository _machineRepository;
        private readonly LineRepository _lineRepository;

        public ClientManager(
            MachineRepository machineRepository,
            LineRepository lineRepository)
        {
            _machineRepository = machineRepository;
            _lineRepository = lineRepository;
        }

        public async Task RegisterAsync(string clientType, TcpClient client)
        {
            switch (clientType)
            {
                case "Unity":
                    UnityClient = client;
                    await SendMachinesToUnityAsync();
                    break;
                case "WPF":
                    WpfClient = client;
                    break;
            }

            Console.WriteLine($"[Client] {clientType} 등록 완료");
        }

        public async Task BroadcastRefreshAsync()
        {
            if (WpfClient == null || !WpfClient.Connected)
                return;

            var machines = await _machineRepository.GetAllMachinesAsync();
            var lines = await _lineRepository.GetAllLinesAsync();

            await _packetSender.SendAsync(WpfClient, Commands.RefreshMachine, machines);
            await _packetSender.SendAsync(WpfClient, Commands.RefreshLine, lines);
        }

        public async Task SendMachinesToUnityAsync()
        {
            if (UnityClient == null || !UnityClient.Connected)
                return;

            var machines = await _machineRepository.GetAllMachinesAsync();
            await _packetSender.SendAsync(UnityClient, Commands.InitMachine, machines);
        }

        public async Task ControlLineToUnityAsync()
        {
            if (UnityClient == null || !UnityClient.Connected)
                return;

            var lines = await _lineRepository.GetAllLinesAsync();
            await _packetSender.SendAsync(UnityClient, Commands.ControlLine, lines);
        }

        public void Remove(TcpClient client)
        {
            if (UnityClient == client)
                UnityClient = null;

            if (WpfClient == client)
                WpfClient = null;
        }
    }
}
