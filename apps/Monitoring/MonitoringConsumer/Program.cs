using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MonitoringConsumer
{
    internal partial class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // PostgreSQL connection
                    var connStr = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION") ??
                                  "Host=postgres;Port=5432;Database=monitoring;Username=postgres;Password=postgres";

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(connStr));

                    // AutoMapper
                    services.AddAutoMapper(cfg => { }, typeof(Program));

                    // Background service
                    services.AddHostedService<KafkaConsumerService>();
                })
                .Build();

            // run migration
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            await host.RunAsync();
        }
    }
}
