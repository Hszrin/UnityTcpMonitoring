using UnityTcpMonitoringShared.Models;

namespace UnityTcpMonitoringServer.Repositories
{
    public interface ILineRepository
    {
        Task<IEnumerable<Line>> GetAllLinesAsync();
        Task<Line?> GetLineAsync(int lineId);
        Task AddLineAsync(Line line);
        Task SaveLineAsync(Line line);
        Task RemoveLineAsync(int lineId);
    }
}
