using UnityTcpMonitoring.Shared.Models;
using UnityTcpMonitoringServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UnityTcpMonitoringApp.Services
{
    public partial class NetworkService
    {
        private PacketSender? _sender;
        private ServerConnection? _connector;
        private CancellationTokenSource? _networkCts;
        public event Action<List<Machine>>? MachineListReceived;
        public event Action<List<Machine>>? MachineListRefreshed; 
        public event Action<List<Line>>? LineListReceived;
        public event Action<List<Line>>? LineListRefreshed;
        public NetworkService()
        {
            var handler = new PacketHandler();

            handler.MachineListReceived += machines =>
                MachineListReceived?.Invoke(machines);

            handler.MachineListRefreshed += machines =>
                MachineListRefreshed?.Invoke(machines);

            handler.LineListReceived += lines =>
                LineListReceived?.Invoke(lines);

            handler.LineListRefreshed += lines =>
                LineListRefreshed?.Invoke(lines);

            _connector = new ServerConnection(handler);
            _connector.Connected += stream =>
            {
                Console.WriteLine($"✅ Connected 발생: {GetHashCode()}");
                _sender = new PacketSender(stream);
                Console.WriteLine($"📡 Sender 생성: {_sender.GetHashCode()}");
            };

            StartLoop();
        }
        public Task InitLineAsync()
        {
            return _sender.SendAsync(new
            {
                Command = Commands.InitLine
            });
        }
        public Task ControlLineAsync(string commandType, int lineId)
        {
            return _sender.SendAsync(new
            {
                Command = Commands.ControlLine,
                Id = lineId,
                Action = commandType
            });
        }
        public Task RefreshLineAsync()
        {
            return _sender.SendAsync(new
            {
                Command = Commands.RefreshLine
            });
        }
        public Task RefreshMachineAsync()
        {
            return _sender.SendAsync(new
            {
                Command = Commands.RefreshMachine,
            });
        }
        public Task RemoveMachineAsync(int id)
        {
            return _sender.SendAsync(new
            {
                Command = Commands.RemoveMachine,
                MachineId = id,
            });
        }
        public Task AddMachineAsync(string machineName, int id)
        {
            return _sender.SendAsync(new
            {
                Command = Commands.AddMachine,
                MachineId = id,
                Name = machineName
            });
        }
        public Task ControlMachineAsync(string commandType, int id)
        {
            return _sender.SendAsync(new
            {
                Command = Commands.ControlMachine,
                MachineId = id,
                Action = commandType
            });
        }
        public Task ImportExcelAsync(byte[] fileBytes)
        {
            return _sender.SendAsync(new
            {
                Command = Commands.ImportExcel,
                FileData = Convert.ToBase64String(fileBytes)
            });
        }
        private void StartLoop()
        {
            _networkCts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                await _connector.ConnectAsync(_networkCts.Token);
            });
        }
    }
}
