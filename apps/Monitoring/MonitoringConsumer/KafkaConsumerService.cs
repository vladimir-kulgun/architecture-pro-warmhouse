using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MonitoringConsumer
{
    internal partial class Program
    {
        public class KafkaConsumerService : BackgroundService
        {
            private readonly ILogger<KafkaConsumerService> _logger;
            private readonly IServiceProvider _serviceProvider;
            private readonly IMapper _mapper;
            private IConsumer<Ignore, string>? _consumer;

            public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IServiceProvider serviceProvider, IMapper mapper)
            {
                _logger = logger;
                _serviceProvider = serviceProvider;
                _mapper = mapper;
            }

            public override Task StartAsync(CancellationToken cancellationToken)
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092",
                    GroupId = "monitoring-consumer-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = true
                };

                _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                var topicName = "temperature-topic";
                _consumer.Subscribe(topicName);

                _logger.LogInformation($"Kafka Consumer connected to '{topicName}'");
                return base.StartAsync(cancellationToken);
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var cr = _consumer!.Consume(stoppingToken);
                            _logger.LogInformation("Consumed message: {Value}", cr.Message.Value);

                            using var scope = _serviceProvider.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                            var temperatureData = JsonSerializer.Deserialize<TemperatureData>(cr.Message.Value);

                            db.TemperatureData.Add(_mapper.Map<TemperatureDataEntity>(temperatureData));
                            await db.SaveChangesAsync(stoppingToken);
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError("Consume error: {Reason}", ex.Error.Reason);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while saving message");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumer stopping...");
                }
                finally
                {
                    _consumer?.Close();
                    _consumer?.Dispose();
                    _logger.LogInformation("Kafka consumer stopped");
                }
            }
        }
    }
}
