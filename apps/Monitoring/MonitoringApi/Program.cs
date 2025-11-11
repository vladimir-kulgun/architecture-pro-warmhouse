using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MonitoringApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration["POSTGRES_CONNECTION"] ??
                                   "Host=postgres;Port=5432;Database=monitoring;Username=postgres;Password=postgres";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || true)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/", () => Results.Redirect("/swagger"));

            app.MapGet("/sensors/{id}/temperature/last", async (int id, AppDbContext db) =>
            {
                var last = await db.TemperatureData
                    .Where(t => t.SensorId == id)
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync();

                return last is not null ? Results.Ok(last) : Results.NotFound($"No data for {id}");
            })
            .WithName("GetLastTemperature")
            .WithOpenApi(op =>
            {
                op.Summary = "Get last temperature reading for a sensor";
                op.Description = "Returns the latest temperature record for the specified sensor ID.";
                return op;
            });

            app.MapGet("/sensors/{id}/temperature/history", async (int id, int limit, AppDbContext db) =>
            {
                if (limit <= 0) limit = 10;
                if (limit > 100) limit = 100;

                var history = await db.TemperatureData
                    .Where(t => t.SensorId == id)
                    .OrderByDescending(t => t.Timestamp)
                    .Take(limit)
                    .ToListAsync();

                return history.Any() ? Results.Ok(history) : Results.NotFound($"No history for {id}");
            })
            .WithName("GetTemperatureHistory")
            .WithOpenApi(op =>
            {
                op.Summary = "Get temperature history for a sensor";
                op.Description = "Returns the last N temperature records (default 10, max 100) for the specified sensor ID.";
                return op;
            });

            await app.RunAsync();
        }
    }
}
