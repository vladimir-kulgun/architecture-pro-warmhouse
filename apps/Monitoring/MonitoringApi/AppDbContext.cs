using Microsoft.EntityFrameworkCore;

namespace MonitoringApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TemperatureDataEntity> TemperatureData => Set<TemperatureDataEntity>();
    }
}
