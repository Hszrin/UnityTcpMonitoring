using Microsoft.EntityFrameworkCore;
using UnityTcpMonitoringShared.Models;

namespace UnityTcpMonitoringServer.Repositories
{
    public class LineRepository : ILineRepository
    {
        public async Task<IEnumerable<Line>> GetAllLinesAsync()
        {
            await using var context = new MesDbContext();
            return await context.Lines.ToListAsync();
        }

        public async Task<Line?> GetLineAsync(int lineId)
        {
            await using var context = new MesDbContext();
            return await context.Lines.FirstOrDefaultAsync(x => x.Id == lineId);
        }

        public async Task AddLineAsync(Line line)
        {
            await using var context = new MesDbContext();

            if (await context.Lines.AnyAsync(x => x.Id == line.Id))
                return;

            await context.Lines.AddAsync(line);
            await context.SaveChangesAsync();
        }

        public async Task UpdateLineStatusAsync(int lineId, string status)
        {
            await using var context = new MesDbContext();
            var line = await context.Lines.FirstOrDefaultAsync(x => x.Id == lineId);

            if (line == null)
                return;

            line.Status = status;

            // 라인 제어 시 소속 설비의 상태도 함께 맞춰 화면과 시뮬레이터의 상태를 일관되게 유지한다.
            var machines = await context.Machines
                .Where(x => x.LineId == lineId)
                .ToListAsync();

            foreach (var machine in machines)
                machine.Status = status;

            await context.SaveChangesAsync();
        }

        public async Task SaveLineAsync(Line line)
        {
            await using var context = new MesDbContext();

            if (line.Id == 0)
                await context.Lines.AddAsync(line);
            else
                context.Lines.Update(line);

            await context.SaveChangesAsync();
        }

        public async Task RemoveLineAsync(int lineId)
        {
            await using var context = new MesDbContext();
            var line = await context.Lines.FindAsync(lineId);

            if (line == null)
                return;

            context.Lines.Remove(line);
            await context.SaveChangesAsync();
        }
    }
}
