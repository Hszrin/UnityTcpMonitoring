using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using UnityTcpMonitoringServer.Repositories;
using UnityTcpMonitoringServer.Services;

namespace UnityTcpMonitoringServer
{
    class Program
    {
        private const int Port = 5000;

        private static readonly MachineRepository _machineRepository = new();
        private static readonly LineRepository _lineRepository = new();
        private static readonly PacketSender _packetSender = new();
        private static readonly ClientManager _clientManager = new(_machineRepository, _lineRepository);
        private static readonly WPFPacketHandler _wpfPacketHandler = new(_machineRepository, _lineRepository, _clientManager);
        private static readonly UnityPacketHandler _unityPacketHandler = new(_machineRepository, _clientManager);

        static async Task Main(string[] args)
        {
            Console.Title = "MiniMES 중앙 백엔드 서버";

            try
            {
                // 서버가 요청을 받기 전에 SQLite 파일과 테이블이 준비되어 있는지 확인한다.
                await using var context = new MesDbContext();
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("[Database] 초기화 완료");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Database] 초기화 실패: {ex}");
                return;
            }

            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine($"[Server] Port {Port}에서 클라이언트 연결 대기 중");

            while (true)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(client));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server] 접속 처리 실패: {ex.Message}");
                }
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string? identity = null;

                try
                {
                    // 연결 직후 첫 줄을 WPF/Unity 식별자로 사용한다.
                    identity = await reader.ReadLineAsync();
                    if (identity is not ("WPF" or "Unity"))
                        return;

                    await _clientManager.RegisterAsync(identity, client);

                    if (identity == "WPF")
                    {
                        var machines = await _machineRepository.GetAllMachinesAsync();
                        var lines = await _lineRepository.GetAllLinesAsync();

                        await _packetSender.SendAsync(client, Commands.InitMachine, machines);
                        await _packetSender.SendAsync(client, Commands.InitLine, lines);
                    }

                    await ProcessCommandAsync(reader, identity);
                }
                finally
                {
                    _clientManager.Remove(client);
                    Console.WriteLine($"[Client] {identity ?? "Unknown"} 연결 종료");
                }
            }
        }

        private static async Task ProcessCommandAsync(StreamReader reader, string clientType)
        {
            try
            {
                while (!reader.EndOfStream)
                {
                    string? commandLine = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(commandLine))
                        continue;

                    var packet = JsonSerializer.Deserialize<PacketHeader>(commandLine);
                    if (packet?.Command == null)
                        continue;

                    Console.WriteLine($"[{clientType}] {packet.Command}");

                    if (clientType == "WPF")
                        await _wpfPacketHandler.ProcessAsync(packet, commandLine);
                    else if (clientType == "Unity")
                        await _unityPacketHandler.ProcessAsync(packet, commandLine);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[{clientType}] 연결 종료: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{clientType}] 패킷 처리 오류: {ex}");
            }
        }
    }

    public class PacketHeader
    {
        public string? Command { get; set; }
    }
}
