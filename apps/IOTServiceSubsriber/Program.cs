using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IOTServiceSubsriber
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient("temperature-client", client =>
                    {
                        var temperatureApiBaseUrl = context.Configuration["TEMPERATURE_API_URL"]
                        ?? "http://temperature-api:8081";

                        client.BaseAddress = new Uri(temperatureApiBaseUrl);
                        client.Timeout = TimeSpan.FromSeconds(10);
                    });

                    services.AddSingleton<ITemperatureClient, TemperatureClient>();

                    // AutoMapper
                    services.AddAutoMapper(cfg => { }, typeof(Program));

                    services.AddHostedService<KafkaProducerService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
