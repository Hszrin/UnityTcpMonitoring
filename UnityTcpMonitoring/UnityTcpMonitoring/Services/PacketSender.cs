using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UnityTcpMonitoringApp.Services
{
    internal class PacketSender
    {
        private readonly NetworkStream _stream;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public PacketSender(NetworkStream stream)
        {
            _stream = stream;
        }

        public async Task SendAsync(object packet)
        {
            string json = JsonSerializer.Serialize(packet) + "\n";
            byte[] data = Encoding.UTF8.GetBytes(json);

            await _lock.WaitAsync();

            try
            {
                await _stream.WriteAsync(data);
                await _stream.FlushAsync();
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
