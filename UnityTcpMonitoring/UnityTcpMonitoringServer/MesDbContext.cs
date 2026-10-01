using Microsoft.EntityFrameworkCore;
using UnityTcpMonitoringShared.Models;

namespace UnityTcpMonitoringServer
{
    public class MesDbContext : DbContext
    {
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Line> Lines { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlite("Data Source=MiniMes.db");
        }
    }
}
