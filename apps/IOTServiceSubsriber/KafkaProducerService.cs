using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IOTServiceSubsriber
{
    public class KafkaProducerService : BackgroundService
    {
        private readonly ITemperatureClient _temperatureClient;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly IMapper _mapper;
        private IProducer<Null, string>? _producer;
        private readonly Random _random = new Random();

        public KafkaProducerService(ILogger<KafkaProducerService> logger, ITemperatureClient temperatureClient, IMapper mapper)
        {
            _logger = logger;
            _temperatureClient = temperatureClient;
            _mapper = mapper;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092",
                ClientId = Dns.GetHostName()
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();

            _logger.LogInformation("Kafka Producer started");
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var sensorId = _random.Next(1, 10);
                var temperatureData = await _temperatureClient.GetTemperatureAsync(sensorId);
                
                if (temperatureData != null)
                {
                    var message = JsonSerializer.Serialize(_mapper.Map<TemperatureData>(temperatureData));

                    try
                    {
                        await _producer!.ProduceAsync("temperature-topic", new Message<Null, string> { Value = message }, stoppingToken);
                        _logger.LogInformation("Produced: {Message}", message);
                    }
                    catch (ProduceException<Null, string> ex)
                    {
                        _logger.LogError("Kafka produce error: {Error}", ex.Error.Reason);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(_random.Next(5)), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _producer?.Flush(cancellationToken);
            _producer?.Dispose();
            _logger.LogInformation("Kafka Producer stopped");
            await base.StopAsync(cancellationToken);
        }
    }
}
