using UnityTcpMonitoring.Shared.Models;
using UnityTcpMonitoringServer;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace UnityTcpMonitoringApp.Services
{
    internal class PacketHandler
    {
        public event Action<List<Machine>>? MachineListReceived;
        public event Action<List<Machine>>? MachineListRefreshed;

        public event Action<List<Line>>? LineListReceived;
        public event Action<List<Line>>? LineListRefreshed;

        public void ProcessPacket(string json)
        {
            var basePacket =
                JsonSerializer.Deserialize<BasePacket>(json);

            if (basePacket == null)
                return;

            switch (basePacket.Command)
            {
                case Commands.InitMachine:
                    {
                        var packet =
                            JsonSerializer.Deserialize<MachineListPacket>(json);

                        MachineListReceived?.Invoke(
                            packet.Data ?? new List<Machine>());

                        break;
                    }

                case Commands.RefreshMachine:
                    {
                        var packet =
                            JsonSerializer.Deserialize<MachineListPacket>(json);

                        MachineListRefreshed?.Invoke(
                            packet.Data ?? new List<Machine>());

                        break;
                    }

                case Commands.InitLine:
                    {
                        var packet =
                            JsonSerializer.Deserialize<LineListPacket>(json);

                        LineListReceived?.Invoke(
                            packet.Data ?? new List<Line>());

                        break;
                    }

                case Commands.RefreshLine:
                    {
                        var packet =
                            JsonSerializer.Deserialize<LineListPacket>(json);

                        LineListRefreshed?.Invoke(
                            packet.Data ?? new List<Line>());

                        break;
                    }
            }
        }
    }
    public class BasePacket
    {
        public string? Command { get; set; }
    }
    public class MachineListPacket
    {
        public List<Machine>? Data { get; set; }
    }
    public class LineListPacket
    {
        public List<Line>? Data { get; set; }
    }
}