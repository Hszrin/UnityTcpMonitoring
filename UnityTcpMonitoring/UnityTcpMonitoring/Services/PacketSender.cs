using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace UnityTcpMonitoring.Services
{
    internal class PacketSender
    {
        private readonly NetworkStream _stream;
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        public PacketSender(NetworkStream stream)
        {
            _stream = stream;
        }

        public async Task SendAsync(object packet)
        {
            string json = JsonSerializer.Serialize(packet) + "\n";
            byte[] data = Encoding.UTF8.GetBytes(json);

            // 하나의 NetworkStream에 여러 쓰기 작업이 겹치지 않도록 송신을 직렬화한다.
            await _sendLock.WaitAsync();

            try
            {
                await _stream.WriteAsync(data);
                await _stream.FlushAsync();
            }
            finally
            {
                _sendLock.Release();
            }
        }
    }
}
