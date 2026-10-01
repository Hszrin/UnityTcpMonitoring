using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace UnityTcpMonitoringServer.Services
{
    internal class PacketSender
    {
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        public async Task SendAsync<T>(TcpClient? client, string command, T data)
        {
            if (client == null || !client.Connected)
                return;

            var packet = new SendPacket<T>
            {
                Command = command,
                Data = data
            };

            string json = JsonSerializer.Serialize(packet);

            // 여러 이벤트가 동시에 발생해도 동일한 TCP 스트림에 패킷이 섞이지 않도록 송신을 직렬화한다.
            await _sendLock.WaitAsync();

            try
            {
                var stream = client.GetStream();
                using var writer = new StreamWriter(
                    stream,
                    new UTF8Encoding(false),
                    1024,
                    leaveOpen: true)
                {
                    AutoFlush = true
                };

                await writer.WriteLineAsync(json);
            }
            finally
            {
                _sendLock.Release();
            }
        }
    }

    public class SendPacket<T>
    {
        public string Command { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
    }
}
