using UnityTcpMonitoringApp.Services;
using System.IO;
using System.Net.Sockets;
using System.Text;

internal class ServerConnection
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly PacketHandler _handler;
    public event Action<NetworkStream>? Connected;
    public NetworkStream? Stream => _stream;
    public ServerConnection(PacketHandler handler)
    {
        _handler = handler;
    }
    public async Task ConnectAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                _client = new TcpClient();

                Console.WriteLine("🌐 중앙 백엔드 서버(Port 5000)에 연결을 시도합니다...");

                await _client.ConnectAsync("127.0.0.1", 5000);

                Console.WriteLine("✅ 중앙 서버 연결 성공!");

                _stream = _client.GetStream();

                Connected?.Invoke(_stream);

                using var writer =
                    new StreamWriter(_stream, Encoding.UTF8)
                    {
                        AutoFlush = true
                    };

                using var reader =
                    new StreamReader(_stream, Encoding.UTF8);

                await writer.WriteLineAsync("WPF");

                while (!token.IsCancellationRequested && _client.Connected)
                {
                    string? jsonLine = await reader.ReadLineAsync();

                    if (jsonLine == null)
                    {
                        Console.WriteLine("⚠️ 서버가 연결을 종료했습니다.");
                        break;
                    }

                    _handler.ProcessPacket(jsonLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ 중앙 서버 연결 끊김: {ex.Message}");
            }
            finally
            {
                _stream = null;
                _client?.Dispose();
                _client = null;
            }

            if (!token.IsCancellationRequested)
                await Task.Delay(5000, token);
        }
    }
    public void Disconnect()
    {
        _stream?.Dispose();
        _client?.Dispose();

        Console.WriteLine("중앙 서버 연결 해제");
        _stream = null;
        _client = null;
    }
}