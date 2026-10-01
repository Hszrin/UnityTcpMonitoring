using System.IO;
using System.Net.Sockets;
using System.Text;

namespace UnityTcpMonitoring.Services
{
    internal class ServerConnection
    {
        private readonly PacketHandler _handler;
        private TcpClient? _client;
        private NetworkStream? _stream;

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
                    Console.WriteLine("[Network] 중앙 서버 연결 시도: 127.0.0.1:5000");

                    await _client.ConnectAsync("127.0.0.1", 5000);
                    _stream = _client.GetStream();

                    Console.WriteLine("[Network] 중앙 서버 연결 성공");
                    Connected?.Invoke(_stream);

                    // JSON 앞에 BOM이 붙지 않도록 송신 Writer는 BOM 없는 UTF-8을 사용한다.
                    using var writer = new StreamWriter(_stream, new UTF8Encoding(false))
                    {
                        AutoFlush = true
                    };
                    using var reader = new StreamReader(_stream, Encoding.UTF8);

                    // 서버가 연결 직후 첫 줄로 클라이언트 역할을 판별한다.
                    await writer.WriteLineAsync("WPF");

                    while (!token.IsCancellationRequested && _client.Connected)
                    {
                        string? jsonLine = await reader.ReadLineAsync();
                        if (jsonLine == null)
                        {
                            Console.WriteLine("[Network] 서버가 연결을 종료했습니다.");
                            break;
                        }

                        _handler.ProcessPacket(jsonLine);
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Network] 중앙 서버 연결 끊김: {ex.Message}");
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
            _stream = null;
            _client = null;
        }
    }
}
