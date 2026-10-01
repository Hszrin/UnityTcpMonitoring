using Microsoft.EntityFrameworkCore;
using UnityTcpMonitoringShared.Models;

namespace UnityTcpMonitoringServer.Repositories
{
    public class MachineRepository : IMachineRepository
    {
        public async Task<IEnumerable<Machine>> GetAllMachinesAsync()
        {
            await using var context = new MesDbContext();
            return await context.Machines.ToListAsync();
        }

        public async Task AddMachineAsync(Machine machine)
        {
            await using var context = new MesDbContext();

            int nextId = await context.Machines.AnyAsync()
                ? await context.Machines.MaxAsync(x => x.Id) + 1
                : 1;

            machine.Id = nextId;

            await context.Machines.AddAsync(machine);
            await context.SaveChangesAsync();
        }

        public async Task AddRangeMachineAsync(List<Machine> machines)
        {
            await using var context = new MesDbContext();

            int nextId = await context.Machines.AnyAsync()
                ? await context.Machines.MaxAsync(x => x.Id) + 1
                : 1;

            foreach (var machine in machines)
                machine.Id = nextId++;

            await context.Machines.AddRangeAsync(machines);
            await context.SaveChangesAsync();
        }

        public async Task RemoveMachineAsync(int machineId)
        {
            await using var context = new MesDbContext();
            var target = await context.Machines.FindAsync(machineId);

            if (target == null)
                return;

            context.Machines.Remove(target);
            await context.SaveChangesAsync();
        }

        public async Task SaveMachineAsync(Machine machine)
        {
            await using var context = new MesDbContext();

            if (machine.Id == 0)
                await context.Machines.AddAsync(machine);
            else
                context.Machines.Update(machine);

            await context.SaveChangesAsync();
        }

        public async Task UpdateMachineStatus(int machineId, string newStatus)
        {
            await using var context = new MesDbContext();
            var machine = await context.Machines.FindAsync(machineId);

            if (machine == null)
                return;

            machine.Status = newStatus;
            await context.SaveChangesAsync();
        }

        public async Task UpdateMachineAsync(
            int machineId,
            string newStatus,
            int productionCount,
            string lastProductionTime)
        {
            await using var context = new MesDbContext();
            var machine = await context.Machines.FindAsync(machineId);

            if (machine == null)
                return;

            machine.Status = newStatus;

            // 늦게 도착한 패킷 때문에 누적 생산량이 이전 값으로 되돌아가지 않도록 증가한 경우만 반영한다.
            if (productionCount > machine.ProductionCount)
            {
                machine.ProductionCount = productionCount;
                machine.LastProductionTime = lastProductionTime;
            }

            await context.SaveChangesAsync();
        }
    }
}
